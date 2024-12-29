using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BombIconPerk;
using GangsAPI.Data;
using GangsAPI.Services;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.DependencyInjection;
using MStatsShared;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Rebel.C4Bomb;

public class C4Behavior(IC4Locale ic4Locale, IRebelService rebelService,
  IServiceProvider provider) : IPluginBehavior, IC4Service {
  public static readonly FakeConVar<bool> CV_GIVE_BOMB = new("css_jb_c4_give",
    "Whether to give a random prisoner a bomb at the beginning of the round.",
    true);

  public static readonly FakeConVar<float> CV_C4_DELAY = new("css_jb_c4_delay",
    "Time in seconds that the bomb takes to explode", .75f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0, 2));

  public static readonly FakeConVar<float> CV_C4_RADIUS =
    new("css_jb_c4_radius", "Bomb explosion radius", 350,
      ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0, 10000));

  public static readonly FakeConVar<float> CV_C4_BASE_DAMAGE =
    new("css_jb_c4_damage", "Base damage to apply", 340, ConVarFlags.FCVAR_NONE,
      new RangeValidator<float>(0, 10000));

  private readonly Dictionary<CC4, C4Metadata> bombs = new();
  private readonly Dictionary<ulong, string> cachedBombIcons = new();

  // EmitSound(CBaseEntity* pEnt, const char* sSoundName, int nPitch, float flVolume, float flDelay)
  private readonly MemoryFunctionVoid<CBaseEntity, string, int, float, float>
    // ReSharper disable once InconsistentNaming
    CBaseEntity_EmitSoundParamsLinux = new(
      "48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 55 41 54 49 89 FC 53 48 89 F3"); // LINUX ONLY.

  private readonly Dictionary<int, int> deathToKiller = new();

  private Timer? bombTimer;

  private bool giveNextRound = true;

  private BasePlugin? plugin;

  public void ClearActiveC4s() {
    bombTimer?.Kill();
    bombTimer = null;
    bombs.Clear();
    deathToKiller.Clear();
  }

  public void TryGiveC4ToPlayer(CCSPlayerController player) {
    var bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
    bombs.Add(bombEntity, new C4Metadata(false));

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

    bombs[bombEntity].IsDetonating = true;

    rebelService.MarkRebel(player);

    bombTimer = plugin.AddTimer(delay, () => detonate(player, bombEntity));
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

  public void DontGiveC4NextRound() { giveNextRound = false; }

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

      StartDetonationAttempt(bombCarrier, CV_C4_DELAY.Value, bomb);
    }
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    ClearActiveC4s();
    refreshBombIcons();

    if (!CV_GIVE_BOMB.Value) return HookResult.Continue;
    if (!giveNextRound) {
      giveNextRound = true;
      return HookResult.Continue;
    }

    TryGiveC4ToRandomTerrorist();
    return HookResult.Continue;
  }

  private void refreshBombIcons() {
    if (API.Gangs == null) return;
    var players = Utilities.GetPlayers()
     .Where(p => p is { IsBot: false, Team: CsTeam.Terrorist })
     .Select(p => new PlayerWrapper(p))
     .ToList();
    cachedBombIcons.Clear();

    var gangStats   = API.Gangs.Services.GetService<IGangStatManager>();
    var gangPlayers = API.Gangs.Services.GetService<IPlayerManager>();
    if (gangStats == null || gangPlayers == null) return;
    Dictionary<int, string> cachedGangBombIcons = new();
    Task.Run(async () => {
      foreach (var player in players) {
        var gangPlayer = await gangPlayers.GetPlayer(player.Steam);
        if (gangPlayer?.GangId == null) continue;


        if (cachedGangBombIcons.TryGetValue(gangPlayer.GangId.Value,
          out var cached)) {
          cachedBombIcons[gangPlayer.Steam] = cached;
          continue;
        }

        var (success, data) =
          await gangStats.GetForGang<BombPerkData>(gangPlayer.GangId.Value,
            BombPerk.STAT_ID);

        if (!success || data == null || data.Equipped == 0)
          cachedGangBombIcons[gangPlayer.GangId.Value] = "";
        else
          cachedGangBombIcons[gangPlayer.GangId.Value] =
            data.Equipped.GetEquipment();
        cachedBombIcons[gangPlayer.Steam] =
          cachedGangBombIcons[gangPlayer.GangId.Value];
      }
    });
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

  // Thank you https://github.com/exkludera/cs2-killfeed-icons/blob/main/src/main.cs
  [GameEventHandler(HookMode.Pre)]
  public HookResult OnPlayerDeath(EventPlayerDeath ev, GameEventInfo info) {
    var victim = ev.Userid;

    if (victim == null || !victim.IsValid) return HookResult.Continue;
    if (!deathToKiller.TryGetValue(victim.Slot, out var killerSlot))
      return HookResult.Continue;

    var killer = Utilities.GetPlayerFromSlot(killerSlot);
    if (killer == null || !killer.IsValid) return HookResult.Continue;

    cachedBombIcons.TryGetValue(killer.SteamID, out var killerIcon);
    if (string.IsNullOrEmpty(killerIcon) || killerIcon == "default")
      killerIcon = "weapon_c4";
    ev.Attacker = killer;
    ev.Weapon   = killerIcon;
    return HookResult.Continue;
  }

  private void detonate(CCSPlayerController player, CC4 bomb) {
    if (!player.IsValid || !player.IsReal() || !player.PawnIsAlive) {
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
    // var lrs = provider.GetRequiredService<ILastRequestManager>();
    foreach (var ct in Utilities.GetPlayers()
     .Where(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true })) {
      // var lr = lrs.GetActiveLR(ct);
      // if (lr != null) {
      //   var otherLr = lrs.GetActiveLR(player);
      //   if (otherLr == null || otherLr != lr) continue;
      // }

      var distanceFromBomb =
        ct.PlayerPawn.Value!.AbsOrigin!.Distance(player.PlayerPawn.Value
         .AbsOrigin!);
      if (distanceFromBomb > CV_C4_RADIUS.Value) continue;

      var damage = CV_C4_BASE_DAMAGE.Value;
      damage *= (CV_C4_RADIUS.Value - distanceFromBomb) / CV_C4_RADIUS.Value;
      float healthRef = ct.PlayerPawn.Value.Health;
      if (healthRef <= damage) {
        deathToKiller[ct.Slot] = player.Slot;
        ct.CommitSuicide(true, true);
        killed++;
      } else {
        ct.PlayerPawn.Value.Health -= (int)damage;
        Utilities.SetStateChanged(ct.PlayerPawn.Value, "CBaseEntity",
          "m_iHealth");
      }
    }

    if (API.Gangs != null && killed > 0) {
      var eco = API.Gangs.Services.GetService<IEcoManager>();
      if (eco != null) {
        var wrapper = new PlayerWrapper(player);
        Task.Run(async ()
          => await eco.Grant(wrapper, killed * 25, reason: "C4 Kill"));
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

  private class C4Metadata(bool isDetonating) {
    public bool IsDetonating { get; set; } = isDetonating;
  }
}