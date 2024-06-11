using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.Logging;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{
    private class JihadBombMetadata(float delay, bool isDetonating) { public float Delay { get; set; } = delay; public bool IsDetonating { get; set; } = isDetonating; }
    private Dictionary<CC4, JihadBombMetadata> _currentActiveJihadC4s = new();

    private IJihadC4Notifications _jihadNotifications;
    private BasePlugin? _basePlugin;

    // EmitSound(CBaseEntity* pEnt, const char* sSoundName, int nPitch, float flVolume, float flDelay)
    private readonly MemoryFunctionVoid<CBaseEntity, string, int, float, float> CBaseEntity_EmitSoundParamsLinux; // LINUX ONLY.

    public JihadC4Behavior(IJihadC4Notifications jihadC4Notifications)
    {
        _jihadNotifications = jihadC4Notifications;
        CBaseEntity_EmitSoundParamsLinux = new("48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 55 41 54 49 89 FC 53 48 89 F3");
    }

    public void Start(BasePlugin basePlugin)
    {
        _basePlugin = basePlugin;
        _basePlugin.RegisterListener<Listeners.OnTick>(PlayerUseC4ListenerCallback);
    }

    private void PlayerUseC4ListenerCallback()
    {
        foreach ((CC4 c4, JihadBombMetadata metadata) in _currentActiveJihadC4s)
        {
            if (metadata.IsDetonating) { continue; }

            CCSPlayerController? bombCarrier = c4.OwnerEntity.Value?.As<CCSPlayerPawn>().Controller.Value?.As<CCSPlayerController>();
            if (bombCarrier == null  || (bombCarrier.Buttons & PlayerButtons.Use) == 0) { continue; }

            CBasePlayerWeapon? activeWeapon = bombCarrier.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
            if (activeWeapon == null || (activeWeapon.Handle != c4.Handle)) { continue; }

            metadata.IsDetonating = true;
            TryDetonateJihadC4(bombCarrier, metadata.Delay, c4);

            TryEmitSound(bombCarrier, "jb.jihad", 1, 1f, 0f);
            _jihadNotifications.PlayerDetonateC4(bombCarrier).ToAllChat();
        }   
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        ClearActiveC4s();
        TryGiveC4ToRandomTerrorist();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDropC4(EventBombDropped @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        CC4? bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);
        if (bombEntity == null) { return HookResult.Continue; } 

        _currentActiveJihadC4s.TryGetValue(bombEntity, out JihadBombMetadata? bombMetadata);
        if (bombMetadata == null) { return HookResult.Continue; }

        // This print to chat requires a NextFrame.
        Server.NextFrame(() => { _jihadNotifications.JIHAD_C4_DROPPED.ToPlayerChat(player); });

        return HookResult.Continue; 

    }

    public void TryGiveC4ToPlayer(CCSPlayerController player)
    {
        CC4 bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
        _currentActiveJihadC4s.Add(bombEntity, new JihadBombMetadata(0.75f, false));

        _jihadNotifications.JIHAD_C4_RECEIVED.ToPlayerChat(player);
        _jihadNotifications.JIHAD_C4_USAGE1.ToPlayerChat(player);
        _jihadNotifications.JIHAD_C4_USAGE2.ToPlayerChat(player);
    }

    public void TryDetonateJihadC4(CCSPlayerController player, float delay, CC4 bombEntity)
    {
        if (_basePlugin == null) { return; }
        Server.RunOnTick(Server.TickCount + (int)(64 * delay), () =>
        {
            if (!player.IsReal() || !player.PawnIsAlive) {
                _currentActiveJihadC4s.TryGetValue(bombEntity, out var metadata);
                if (metadata != null)
                {
                    metadata.IsDetonating = false; // So other players can detonate it.
                }
                return;
            } // Cancel the detonation if the player died. 

            CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
            particleSystemEntity.EffectName = "particles/explosions_fx/explosion_c4_500.vpcf";
            particleSystemEntity.StartActive = true;

            particleSystemEntity.Teleport(player.PlayerPawn!.Value!.AbsOrigin!, new QAngle(), new Vector());
            particleSystemEntity.DispatchSpawn();

            bool hadC4 = TryRemoveWeaponC4(player); // We want to remove the C4 from their inventory b4 we detonate the bomb (if they have it).

            /* Calculate damage here, only applies to alive CTs. */
            foreach (CCSPlayerController potentialTarget in Utilities.GetPlayers().Where((p) => p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
            {
                float distanceFromBomb = potentialTarget.PlayerPawn!.Value!.AbsOrigin!.Distance(player.PlayerPawn.Value.AbsOrigin!);
                if (distanceFromBomb > 350f) { continue; } // 350f = "bombRadius"

                float damage = 340f;
                damage *= (350f - distanceFromBomb) / 350f;
                float healthRef = potentialTarget.PlayerPawn.Value.Health;
                if (healthRef <= damage)
                {
                    potentialTarget.CommitSuicide(true, true);
                } else
                {
                    potentialTarget.PlayerPawn.Value.Health -= (int)damage;
                    Utilities.SetStateChanged(potentialTarget, "CBaseEntity", "m_iHealth");
                }
            }

            // Emit the sound first.
            TryEmitSound(player, "jb.jihadExplosion", 1, 1f, 0f);

            if (!hadC4) // If they didn't have the C4 that means it's on the ground, so let's remove it here.
            {
                if (bombEntity.IsValid) { bombEntity.Remove(); }
            }

            player.CommitSuicide(true, true);
            _currentActiveJihadC4s.Remove(bombEntity);

        });

    }

    public void TryGiveC4ToRandomTerrorist()
    {
        List<CCSPlayerController> validTerroristPlayers;
        int numOfTerrorists;
        int randomIndex;

        Server.RunOnTick(Server.TickCount + 256, () => // Wait 4 secs before going thru
        {
            validTerroristPlayers = Utilities.GetPlayers()
                .Where(player => player.Team == CsTeam.Terrorist && player.PawnIsAlive && !player.IsBot).ToList();
            numOfTerrorists = validTerroristPlayers.Count;
            if (numOfTerrorists == 0)
            {
                _basePlugin!.Logger.LogInformation(
                    "Tried to give Jihad C4 at round start but there were no valid players to give it to.");
                return;
            }

            Random rnd = new();
            randomIndex = rnd.Next(numOfTerrorists);
            if (!validTerroristPlayers[randomIndex].IsValid)
            {
                TryGiveC4ToRandomTerrorist();
                return;
            }
            TryGiveC4ToPlayer(validTerroristPlayers[randomIndex]);
        });
    }

    public void ClearActiveC4s()
    {
        _currentActiveJihadC4s.Clear();
    }

    private void TryEmitSound(CBaseEntity entity, string soundEventName, int pitch, float volume, float delay)
    {
        CBaseEntity_EmitSoundParamsLinux.Invoke(entity, soundEventName, pitch, volume, delay);
    }

    // Returns whether the weapon c4 was in their inventory or not.
    private bool TryRemoveWeaponC4(CCSPlayerController player)
    {
        if (player.PlayerPawn.Value?.WeaponServices == null) { return false; }
        foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
        {
            if (weapon.Value == null) { continue; }
            if (weapon.Value.DesignerName == "weapon_c4")
            {
                weapon.Value.Remove();
                return true;
            }
        }
        return false;
    }

}
