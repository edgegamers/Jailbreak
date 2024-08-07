﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using MStatsShared;

namespace Jailbreak.Rebel.C4Bomb;

public class C4Behavior(IC4Locale ic4Locale, IRebelService rebelService)
  : IPluginBehavior, IC4Service {
  private readonly Dictionary<CC4, C4Metadata> bombs = new();

  // EmitSound(CBaseEntity* pEnt, const char* sSoundName, int nPitch, float flVolume, float flDelay)
  private readonly MemoryFunctionVoid<CBaseEntity, string, int, float, float>
    // ReSharper disable once InconsistentNaming
    CBaseEntity_EmitSoundParamsLinux = new(
      "48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 55 41 54 49 89 FC 53 48 89 F3"); // LINUX ONLY.

  private BasePlugin? plugin;

  public void ClearActiveC4s() { bombs.Clear(); }

  public void TryGiveC4ToPlayer(CCSPlayerController player) {
    var bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
    bombs.Add(bombEntity, new C4Metadata(0.75f, false));

    ic4Locale.JihadC4Received.ToChat(player);
    ic4Locale.JihadC4Usage1.ToChat(player);
  }

  public void StartDetonationAttempt(CCSPlayerController player, float delay,
    CC4 bombEntity) {
    if (plugin == null) return;
    var pos = player.Pawn.Value?.AbsOrigin;
    if (pos != null)
      API.Stats?.PushStat(new ServerStat("JB_BOMB_ATTEMPT",
        $"{pos.X:F2} {pos.Y:F2} {pos.Z:F2}"));

    tryEmitSound(player, "jb.jihad", 1, 1f, 0f);

    bombs[bombEntity].Delay        = delay;
    bombs[bombEntity].IsDetonating = true;

    rebelService.MarkRebel(player);

    Server.RunOnTick(Server.TickCount + (int)(64 * delay),
      () => detonate(player, bombEntity));
  }

  public void TryGiveC4ToRandomTerrorist() {
    plugin!.AddTimer(1, () => {
      var validTerroristPlayers = Utilities.GetPlayers()
       .Where(player => player is {
          Team       : CsTeam.Terrorist,
          PawnIsAlive: true,
          IsBot      : false,
          IsValid    : true
        })
       .ToList();
      var numOfTerrorists = validTerroristPlayers.Count;
      if (numOfTerrorists == 0) return;

      Random rnd         = new();
      var    randomIndex = rnd.Next(numOfTerrorists);
      TryGiveC4ToPlayer(validTerroristPlayers[randomIndex]);
    });
  }

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    plugin.RegisterListener<Listeners.OnTick>(playerUseC4ListenerCallback);
  }

  private void playerUseC4ListenerCallback() {
    foreach (var (bomb, meta) in bombs) {
      if (!bomb.IsValid) continue;
      if (meta.IsDetonating) continue;

      var bombCarrier = bomb.OwnerEntity.Value?.As<CCSPlayerPawn>()
       .Controller.Value?.As<CCSPlayerController>();
      if (bombCarrier == null || !bombCarrier.IsValid
        || (bombCarrier.Buttons & PlayerButtons.Use) == 0)
        continue;

      var activeWeapon = bombCarrier.PlayerPawn.Value?.WeaponServices
      ?.ActiveWeapon.Value;
      if (activeWeapon == null || !activeWeapon.IsValid
        || activeWeapon.Handle != bomb.Handle)
        continue;

      StartDetonationAttempt(bombCarrier, meta.Delay, bomb);
    }
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    ClearActiveC4s();
    TryGiveC4ToRandomTerrorist();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDropC4(EventBombDropped @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    var bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);
    if (bombEntity == null) return HookResult.Continue;

    bombs.TryGetValue(bombEntity, out var bombMetadata);
    if (bombMetadata == null) return HookResult.Continue;

    if (bombMetadata.IsDetonating) {
      bombEntity.Remove();
      return HookResult.Stop;
    }

    return HookResult.Continue;
  }

  private void detonate(CCSPlayerController player, CC4 bomb) {
    if (!player.IsValid || !player.IsReal() || !player.PawnIsAlive) {
      bombs.TryGetValue(bomb, out _);
      if (bomb.IsValid) bomb.Remove();
      bombs.Remove(bomb);
      return;
    }

    tryEmitSound(player, "jb.jihadExplosion", 1, 1f, 0f);
    var particleSystemEntity =
      Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
    particleSystemEntity.EffectName =
      "particles/explosions_fx/explosion_c4_500.vpcf";
    particleSystemEntity.StartActive = true;

    particleSystemEntity.Teleport(player.PlayerPawn.Value!.AbsOrigin!,
      new QAngle(), new Vector());
    particleSystemEntity.DispatchSpawn();

    var killed = 0;
    /* Calculate damage here, only applies to alive CTs. */
    foreach (var ct in Utilities.GetPlayers()
     .Where(p => p is {
        Team: CsTeam.CounterTerrorist, PawnIsAlive: true, IsValid: true
      })) {
      var distanceFromBomb =
        ct.PlayerPawn.Value!.AbsOrigin!.Distance(player.PlayerPawn.Value
         .AbsOrigin!);
      if (distanceFromBomb > 350f) continue;

      // 350f = "bombRadius"
      var damage = 340f;
      damage *= (350f - distanceFromBomb) / 350f;
      float healthRef = ct.PlayerPawn.Value.Health;
      if (healthRef <= damage) {
        ct.CommitSuicide(true, true);
        killed++;
      } else {
        ct.PlayerPawn.Value.Health -= (int)damage;
        Utilities.SetStateChanged(ct.PlayerPawn.Value, "CBaseEntity",
          "m_iHealth");
      }
    }

    API.Stats?.PushStat(new ServerStat("JB_BOMB_EXPLODED", killed.ToString()));

    // If they didn't have the C4 make sure to remove it.
    player.CommitSuicide(true, true);
    bombs.Remove(bomb);
  }

  private void tryEmitSound(CBaseEntity entity, string soundEventName,
    int pitch, float volume, float delay) {
    CBaseEntity_EmitSoundParamsLinux.Invoke(entity, soundEventName, pitch,
      volume, delay);
  }

  private class C4Metadata(float delay, bool isDetonating) {
    public float Delay { get; set; } = delay;
    public bool IsDetonating { get; set; } = isDetonating;
  }
}