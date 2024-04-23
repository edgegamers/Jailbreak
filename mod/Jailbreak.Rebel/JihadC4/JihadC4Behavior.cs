using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Rebel;
using System.Runtime.InteropServices;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{

    // Importantly the Player argument CAN be null!
    private class JihadBombMetadata(CCSPlayerController? player, float delay) { public CCSPlayerController? Player { get; set; } = player; public float Delay { get; set; } = delay; }

    private IJihadC4Notifications _jihadNotifications;
    private BasePlugin _basePlugin;

    // Key presents any active Jihad C4 in the world. Values represent metadata about that Jihad C4.
    private Dictionary<CC4, JihadBombMetadata> _currentActiveJihadC4s = new();

    // Windows AND Linux... :)
    // Todo actually test this in the various funcs (the sig does work though)
    private readonly MemoryFunctionVoid<nint, string, int, float, float> CBaseEntity_EmitSoundParams = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) ? new("\\x48\\xB8\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x55\\x48\\x89\\xE5\\x41\\x55\\x41\\x54\\x49\\x89\\xFC\\x53\\x48\\x89\\xF3") : new("\\x48\\x8B\\xC4\\x48\\x89\\x58\\x10\\x48\\x89\\x70\\x18\\x55\\x57\\x41\\x56\\x48\\x8D\\xA8\\x08\\xFF\\xFF\\xFF");

    // todo add notification support here
    public JihadC4Behavior(IJihadC4Notifications jihadC4Notifications)
    {
        _jihadNotifications = jihadC4Notifications;   
    }

    public void Start(BasePlugin basePlugin)
    {
        _basePlugin = basePlugin;
        // Register an OnTick listener to listen for +use
        _basePlugin.RegisterListener<Listeners.OnTick>(PlayerUseC4ListenerCallback);
    }

    // TODO HANDLE WHEN PLAYER LEAVES AND STUFF LIKE THAT

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

            CPlayer_WeaponServices? weaponServices = TryGetWeaponServices(player);
            if (weaponServices == null || weaponServices.ActiveWeapon == null || weaponServices.ActiveWeapon.Value == null || !weaponServices.ActiveWeapon.Value.IsValid) { continue; }
            
            // Check if the currently held and "+used" item is our C4
            if (!weaponServices.ActiveWeapon!.Value!.DesignerName.Equals("weapon_c4")) { continue; }

            CC4 bombEntity = new CC4(weaponServices.ActiveWeapon!.Value!.Handle);
            _currentActiveJihadC4s.Remove(bombEntity);

            //player.ExecuteClientCommandFromServer("kill"); UNCOMMENT LATER ONCE YOU'RE DONE WITH PLUGIN
            TryDetonateJihadC4(player, metadata.Delay);
            Console.WriteLine("4");

            _jihadNotifications.PlayerDetonateC4(player).ToAllChat();

            Server.NextFrame(bombEntity.Remove); // todo check if this actually works

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
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerDropC4(EventBombDropped @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        CC4 bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);

        // We check this as obviously we're only concerned with C4's that are in our dictionary
        if (!_currentActiveJihadC4s.ContainsKey(bombEntity)) { return HookResult.Continue; }

        // If a jihad bomb is dropped then the player entry in the dictionary needs to be nulled. We will set it again when another player picks it up.
        _currentActiveJihadC4s[bombEntity].Player = null;
        // This requires a nextframe because apparently some Valve functions don't like printing inside of them.
        Server.NextFrame(() => { _jihadNotifications.JIHAD_C4_DROPPED.ToPlayerChat(player); });

        return HookResult.Continue;

    }

    /// <summary>
    /// This function listens to when a player picks up any weapon_c4 item. If it is a Jihad C4 (which can easily be checked) then
    /// we assign the Jihad C4's "owner" to that player. 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerPickupC4(EventBombPickup @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        CPlayer_WeaponServices? weaponServices = TryGetWeaponServices(player);
        if (weaponServices == null) { Console.WriteLine("Weaponservice was null in OnPlayerPickupC4"); return HookResult.Continue; }

        CC4 bombEntity = new CC4(weaponServices.MyWeapons.Last()!.Value!.Handle); // The last item in the weapons list is the last item the player picked up, apparently
        if (!_currentActiveJihadC4s.ContainsKey(bombEntity)) { return HookResult.Continue; }

        _currentActiveJihadC4s[bombEntity].Player = player;
        _jihadNotifications.JIHAD_C4_PICKUP.ToPlayerChat(player);

        return HookResult.Continue;

    }

    /// <summary>
    /// A useful function to reset the Jihad C4 state. A better solution might be to clear the list on round START instead.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        _currentActiveJihadC4s.Clear();
        return HookResult.Continue;
    }

    /// <summary>
    /// The purpose of this event handler is to safely handle when a player with an active Jihad C4 leaves the server.
    /// It ensures that the Jihad C4 that is dropped when they are disconnected is still useable.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerLeave(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (_currentActiveJihadC4s.Count == 0) { return HookResult.Continue; }

        CCSPlayerController? player = @event.Userid;
        if (player == null || !player.IsValid) { return HookResult.Continue; }

        foreach (JihadBombMetadata metadata in _currentActiveJihadC4s.Values)
        {
            if (metadata.Player == player)
            {
                metadata.Player = null;
            }
        }

        return HookResult.Continue;

    }

    /// <summary>
    /// Self-explanatory function. This function registers the given C4 and the player who received the bomb.
    /// If this function fails then nothing will happen.
    /// </summary>
    /// <param name="player"></param>
    public void TryGiveC4ToPlayer(CCSPlayerController player)
    {
        CC4 bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
        _currentActiveJihadC4s[bombEntity] = new JihadBombMetadata(player, 1.0f);
        _jihadNotifications.JIHAD_C4_RECEIVED.ToPlayerChat(player);
    }

    // Not using _notifications.PlayerDetonateC4() here, as I invoked that in the +use callback already
    /// <summary>
    /// This function creates a Jihad C4 styled explosion centred at the player's AbsOrigin. This function doesn't check if 
    /// the player even has a C4, it is simply used to create the explosion!
    /// </summary>
    /// <param name="player"></param>
    /// <param name="delay"></param>
    public void TryDetonateJihadC4(CCSPlayerController player, float delay)
    {
        /* PARTICLE EXPLOSION */
        CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
        particleSystemEntity.EffectName = "particles/explosions_fx/explosion_c4_500.vpcf";
        particleSystemEntity.StartActive = true;

        particleSystemEntity.Teleport(player.PlayerPawn!.Value!.AbsOrigin!, new QAngle(), new Vector());
        particleSystemEntity.DispatchSpawn();

        /* PHYS EXPLPOSION, FOR PUSHING PLAYERS */
        /* Values can always be tweaked, the important ones are Magnitude and Pushscale */
        /* Currently this physics explosion will affect players through walls, can easily change this though */
        CPhysExplosion envPhysExplosionEntity = Utilities.CreateEntityByName<CPhysExplosion>("env_physexplosion")!;

        envPhysExplosionEntity.Spawnflags = 1 << 1; // Push players flag set to true!
        envPhysExplosionEntity.ExplodeOnSpawn = true;
        envPhysExplosionEntity.Magnitude = 50f;
        envPhysExplosionEntity.PushScale = 3.5f;
        envPhysExplosionEntity.Radius = 340f; // As per old code.

        envPhysExplosionEntity.Teleport(player.PlayerPawn.Value!.AbsOrigin!, new QAngle(), new Vector());
        envPhysExplosionEntity.DispatchSpawn();

    }

    private CPlayer_WeaponServices? TryGetWeaponServices(CCSPlayerController? player)
    {
        if (player == null) { return null; }
        if (player.PlayerPawn == null || player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null) { return null; }
        return player.PlayerPawn.Value.WeaponServices;
    }

}
