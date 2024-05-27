using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{

    // Importantly the Player argument CAN be null!
    private class JihadBombMetadata(CCSPlayerController? player, float delay) { public CCSPlayerController? Player { get; set; } = player; public float Delay { get; set; } = delay; }
    // Key presents any active Jihad C4 in the world. Values represent metadata about that Jihad C4.
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

        // Register an OnTick listener to listen for +use
        _basePlugin.RegisterListener<Listeners.OnTick>(PlayerUseC4ListenerCallback);


    }

    /// <summary>
    /// This function listens to when a player with an active Jihad C4 detonates their bomb by doing +use.
    /// It will call another function to actually produce the Jihad C4 styled explosion, and handles removing the player 
    /// from the list of active C4's, to name one thing.
    /// </summary>
    private void PlayerUseC4ListenerCallback()
    {

        foreach (JihadBombMetadata metadata in _currentActiveJihadC4s.Values)
        {
            CCSPlayerController? player = metadata.Player;
            if (player == null) { continue; }

            // is the use button currently active? 
            if ((player.Buttons & PlayerButtons.Use) == 0) { continue; }

            CPlayer_WeaponServices? weaponServices = player.PlayerPawn?.Value?.WeaponServices;
            if (weaponServices == null) { continue; }

            // Check if the currently held and "+used" item is our C4
            string? heldItemDesignerName = weaponServices.ActiveWeapon?.Value?.DesignerName;
            if (heldItemDesignerName == null) { continue; }
            if (!heldItemDesignerName.Equals("weapon_c4")) { continue; }

            CC4 bombEntity = new CC4(weaponServices.ActiveWeapon!.Value!.Handle);
            _currentActiveJihadC4s.Remove(bombEntity);

            // This will deal with the explosion and ensuring the detonator is killed, as well as removing the bomb entity.
            TryDetonateJihadC4(player, metadata.Delay, bombEntity);

            TryEmitSound(player, "jb.jihad", 1, 1f, 0f);
            _jihadNotifications.PlayerDetonateC4(player).ToAllChat();

        }
    }

    // Todo please remove later!!
    [ConsoleCommand("css_d", "debug cmd")]
    public void Command_Debug(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null || !executor.IsValid) { return; }
        TryGiveC4ToPlayer(executor);
    }

    /// <summary>
    /// This function importantly allows players who have a Jihad C4 to pass it on to other Terrorists. Additionally it deals with
    /// the edge case where a player dies with a Jihad C4, as this function is still called when that happens.
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerDropC4(EventBombDropped @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        CC4? bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);
        if (bombEntity == null) { return HookResult.Continue; } // I mean this should never be the case...

        // We check this as obviously we're only concerned with C4's that are in our dictionary
        _currentActiveJihadC4s.TryGetValue(bombEntity, out JihadBombMetadata? bombMetadata);
        if (bombMetadata == null) { return HookResult.Continue; }

        // If a jihad bomb is dropped then the player entry in the dictionary needs to be nulled. We will set it again when another player picks it up.
        bombMetadata.Player = null;

        // This requires a nextframe because apparently some Valve functions don't like printing inside of them.
        Server.NextFrame(() => { _jihadNotifications.JIHAD_C4_DROPPED.ToPlayerChat(player); });

        return HookResult.Continue; 

    }

    /// <summary>
    /// This function listens to when a player picks up any weapon_c4 item. If it is a Jihad C4 (which can easily be checked) then
    /// we assign the Jihad C4's "owner" to that player. 
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerPickupC4(EventBombPickup @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        CPlayer_WeaponServices? weaponServices = player.PlayerPawn?.Value?.WeaponServices;
        if (weaponServices == null) { return HookResult.Continue; }

        CC4 bombEntity = new CC4(weaponServices.MyWeapons.Last()!.Value!.Handle); // The last item in the weapons list is the last item the player picked up, apparently.
        if (!_currentActiveJihadC4s.ContainsKey(bombEntity)) { return HookResult.Continue; }

        _currentActiveJihadC4s.TryGetValue(bombEntity, out JihadBombMetadata? bombMetadata);
        if (bombMetadata == null) { return HookResult.Continue; }

        bombMetadata.Player = player;
        _jihadNotifications.JIHAD_C4_PICKUP.ToPlayerChat(player);

        return HookResult.Continue;

    }

    /// <summary>
    /// Invokes the method that attempts to give a jihad C4 to a random terrorist, done at the start of the round.
    /// </summary>
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _currentActiveJihadC4s.Clear();
        TryGiveC4ToRandomTerrorist();
        return HookResult.Continue;
    }

    /// <summary>
    /// The purpose of this event handler is to safely handle when a player with an active Jihad C4 leaves the server.
    /// It ensures that the Jihad C4 that is dropped when they are disconnected is still useable.
    /// </summary>
    [GameEventHandler]
    public HookResult OnPlayerLeave(EventPlayerDisconnect @event, GameEventInfo info)
    {

        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        // get the bomb metadata where the Player variable is assigned to the player who disconnected
        JihadBombMetadata? metadata = _currentActiveJihadC4s.Values.DistinctBy((metadata) => metadata.Player == player).FirstOrDefault();
        if (metadata == null) { return HookResult.Continue; }

        metadata.Player = null; // then null it.

        return HookResult.Continue;

    }

    /// <summary>
    /// Self-explanatory function. This function registers the given C4 and the player who received the bomb.
    /// If this function fails then nothing will happen.
    /// </summary>
    /// <param name="player">The player who should receive the Jihad C4.</param>
    public void TryGiveC4ToPlayer(CCSPlayerController player)
    {
        foreach (var metadata in _currentActiveJihadC4s.Values)
        { if (metadata.Player == player) { return; } }

        CC4 bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
        _currentActiveJihadC4s[bombEntity] = new JihadBombMetadata(player, 1.0f);
        _jihadNotifications.JIHAD_C4_RECEIVED.ToPlayerChat(player);
    }

    // Not using _notifications.PlayerDetonateC4() here, as I invoked that in the +use callback already
    /// <summary>
    /// This function creates a Jihad C4 styled explosion centred at the player's AbsOrigin. This function doesn't check if 
    /// the player even has a C4, it is simply used to create the explosion!
    /// </summary>
    /// <param name="player">The explosion will be generated at the player's AbsOrigin.</param>
    /// <param name="delay">The delay, in seconds, to wait before creating our explosion!</param>
    /// <param name="bombEntity">By default this is null, but if it is NOT null, then this entity will be removed after the explosion is generated.</param>
    public void TryDetonateJihadC4(CCSPlayerController player, float delay, CC4? bombEntity = null)
    {
        if (_basePlugin == null) { return; }
        _basePlugin.AddTimer(delay, () =>
        {
            /* PARTICLE EXPLOSION */
            CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
            particleSystemEntity.EffectName = "particles/explosions_fx/explosion_c4_500.vpcf";
            particleSystemEntity.StartActive = true;

            particleSystemEntity.Teleport(player.PlayerPawn!.Value!.AbsOrigin!, new QAngle(), new Vector());
            particleSystemEntity.DispatchSpawn();
            /* END */

            /* PHYS EXPLPOSION, FOR PUSHING PLAYERS */
            /* Values can always be tweaked, the important ones are Magnitude and Pushscale */
            /* Currently this physics explosion will affect players through walls, this can be changed though. */
            CPhysExplosion envPhysExplosionEntity = Utilities.CreateEntityByName<CPhysExplosion>("env_physexplosion")!;

            envPhysExplosionEntity.Spawnflags = 1 << 1; // Push players flag set to true!
            envPhysExplosionEntity.ExplodeOnSpawn = true;
            envPhysExplosionEntity.Magnitude = 50f; // I have tweaked these values
            envPhysExplosionEntity.PushScale = 3.5f; // I have tweaked these values
            envPhysExplosionEntity.Radius = 340f; // As per the old code.

            envPhysExplosionEntity.Teleport(player.PlayerPawn.Value!.AbsOrigin!, new QAngle(), new Vector());
            envPhysExplosionEntity.DispatchSpawn();
            /* END */

            /* Calculate damage here. */
            /* Don't waste time calculating stuff for dead players. */
            /* Also, Utilities.GetPlayers() returns valid players anyway, so no need to check for that. */
            foreach (CCSPlayerController potentialTarget in Utilities.GetPlayers().Where((p) => p.PawnIsAlive))
            {
                float distanceFromBomb = potentialTarget.PlayerPawn!.Value!.AbsOrigin!.Distance(player.PlayerPawn!.Value!.AbsOrigin!);
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

            /* Finally play our sound and remove the bomb */
            TryEmitSound(player, "jb.jihadExplosion", 1, 1f, 0f);
            bombEntity?.Remove();

        });

    }

    /// <summary>
    /// Self-explanatory.
    /// </summary>
    public void TryGiveC4ToRandomTerrorist()
    {
        List<CCSPlayerController> validTerroristPlayers = Utilities.GetPlayers().Where(player => player.Team == CsTeam.Terrorist && player.PawnIsAlive && !player.IsBot).ToList();
        int numOfTerrorists = validTerroristPlayers.Count;

        if (numOfTerrorists == 0) { _basePlugin!.Logger.LogInformation("Tried to give Jihad C4 at round start but there were no valid players to give it to."); return; }

        Random rnd = new();
        int randomIndex = rnd.Next(numOfTerrorists);

        TryGiveC4ToPlayer(validTerroristPlayers[randomIndex]);

    }

    // No error checking unfortunately apart from the default error that's thrown, sorry jii :)
    private void TryEmitSound(CBaseEntity entity, string soundEventName, int pitch, float volume, float delay)
    {
        CBaseEntity_EmitSoundParamsLinux.Invoke(entity, soundEventName, pitch, volume, delay);
    }

}
