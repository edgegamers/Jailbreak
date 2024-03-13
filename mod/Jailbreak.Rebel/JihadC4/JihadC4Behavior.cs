using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Behaviors;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Generic;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Net.NetworkInformation;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{

    // use services we want here
    private readonly IJihadC4Notifications _jihadC4Notifications;
    private readonly ICoroutines _coroutines;
    private BasePlugin? _basePlugin;

    // any entry in this list implies the player has an active jihad c4 in their pawn inventory
    private Dictionary<CCSPlayerController, JihadC4Metadata> _activeJihadC4Players;

    private record struct JihadC4Metadata(nint bombEntityPointer, float Delay);

    public JihadC4Behavior(IJihadC4Notifications jihadC4Notifications, ICoroutines coroutines)
    {
        _jihadC4Notifications = jihadC4Notifications;
        _coroutines = coroutines;

        _activeJihadC4Players = new Dictionary<CCSPlayerController, JihadC4Metadata>();
    }


    public void Start(BasePlugin parent)
    {
        _basePlugin = parent;
        if (_basePlugin == null) { throw new NullReferenceException("_basePlugin reference was null...?"); }
    }

    // debug cmd remove later
    [ConsoleCommand("css_d", "")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Debug(CCSPlayerController? player, CommandInfo info)
    {

        if (player == null) { return; }
        if (player.AbsOrigin == null) { return; }

        GiveJihadC4ToTerrorist(player);

    }

    [ConsoleCommand("css_delay", "Sets a delay on the Jihad C4. Must be holding it out. Usage: !delay <seconds>")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Delay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) { return; }
        if (info.ArgCount <= 1) { TrySetDelayJihadC4(player, 0.0f); return; } // I guess by default no args = 0 seconds

        try
        {
            float delay = Convert.ToSingle(info.GetArg(1));
            TrySetDelayJihadC4(player, delay);
            _jihadC4Notifications.JihadCurrentDelay(delay).ToPlayerChat(player);
        } catch (Exception)
        {
            _jihadC4Notifications.JIHAD_DELAY_CMD_USAGE.ToPlayerChat(player);
            return;
        }


    }

    /// <summary>
    /// The Terrorist with the Jihad C4 is classed as "Using" the C4 if they drop it into the world with the G key (or whatever
    /// it's bound to).
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnTerroristUseJihadC4(EventBombDropped @event, GameEventInfo info)
    {
        if (@event.Userid == null || !@event.Userid.IsValid) { return HookResult.Continue; }
        CC4 bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);
        if (bombEntity.Entity == null || bombEntity.Entity.Name == null || !bombEntity.Entity.Name.StartsWith("jihad_c4") ) { return HookResult.Continue; }
        if (!_activeJihadC4Players.Keys.Contains(@event.Userid)) { return HookResult.Continue; }
        // else the JihadC4 is active for the specified player and we can invoke TryBlowUpJihadC4()
        JihadC4Metadata m = _activeJihadC4Players[@event.Userid];
        Server.NextFrame(() =>
        {
            bombEntity.CanBePickedUp = false;
        });
        TryBlowUpJihadC4(bombEntity, @event.Userid);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnTerroristEquipJihadC4(EventItemEquip @event, GameEventInfo info)
    {

        try
        {
            Console.WriteLine($"Name of entity if any: {Utilities.GetEntityFromIndex<CC4>((int)@event.Defindex).Entity!.Name}");
        } catch (Exception ex)
        {

        }

        if (@event.Userid == null || !@event.Userid.IsValid) { return HookResult.Continue; }
        if (@event.Weptype != ((uint)CSWeaponType.WEAPONTYPE_C4)) { return HookResult.Continue; }
        if (!_activeJihadC4Players.Keys.Contains(@event.Userid)) { return HookResult.Continue; }
        
        // else the currently held out item is a weapon_c4 and the player holding it has the capabilities to blow it up
        // thanks Roflmuffin 
        CCSPlayerController player = @event.Userid;
        Server.NextFrame(() =>
        {
            if (player == null) { return; }
            if (!player.IsValid) { return; }
            _jihadC4Notifications.JihadCurrentDelay(_activeJihadC4Players[player].Delay).ToPlayerChat(player);
        });

        return HookResult.Continue;
    }

    public void GiveJihadC4ToRandomTerrorist()
    {
        throw new NotImplementedException();
    }

    public void GiveJihadC4ToTerrorist(CCSPlayerController terrorist)
    {

        // this is a pointer... nint maps to System.IntPtr
        nint bombEntityPointer = terrorist.GiveNamedItem(CsItem.C4);

        CC4 entityC4 = new CC4(bombEntityPointer);
        entityC4.Entity!.Name = "jihad_c4";

        // todo find function signature of item_pickup 

        

        // by default the bomb explodes immediately
        JihadC4Metadata metadata = new JihadC4Metadata(bombEntityPointer, 0.0f);
        _activeJihadC4Players.Add(terrorist, metadata);

        _jihadC4Notifications.JIHAD_GIVEN_BOMB.ToPlayerChat(terrorist);

    }


    // Should blow up the player only at the given location 
    public void TryBlowUpJihadC4(CC4 bombEntity, CCSPlayerController terrorist)
    {
        if (!_activeJihadC4Players.ContainsKey(terrorist)) {  return; }

        float delay = _activeJihadC4Players[terrorist].Delay;
        _activeJihadC4Players.Remove(terrorist);

        // todo play WA LA LA LA sound now

        // Then we just want the position of the C4 in the world and remove it after the timer 
        _coroutines.Round(() =>
        {
            // TODO use env_physexplosion TO APPLY FORCE!
            // todo detonate then remove
            CEnvExplosion explosionEffectEntity = Utilities.CreateEntityByName<CEnvExplosion>("env_explosion")!;
            //explosionEffectEntity.Flags |= (1 << 4) | (1 << 7) | (1 << 14); // no black decal scorch, random orientation, do DMG_GENERIC damage
            //explosionEffectEntity.Magnitude = 300;
            //explosionEffectEntity.RadiusOverride = 300;

            //explosionEffectEntity.Teleport(bombEntity.AbsOrigin!, new QAngle(), new Vector());
            explosionEffectEntity.AcceptInput("Explode"); // explode the bomb !! 

            // todo play noise at AbsOrigin location ?

            bombEntity.Remove(); // this is why we're getting crashes 
        }, delay + 0.01f);

    }

    public void TryDefuseJihadC4(CCSPlayerController terrorist)
    {
        throw new NotImplementedException();
    }

    public void TrySetDelayJihadC4(CCSPlayerController terrorist, float delay)
    {
        if (!_activeJihadC4Players.ContainsKey(terrorist)) { return; }
        JihadC4Metadata metadataCopy = _activeJihadC4Players[terrorist];
        metadataCopy.Delay = delay;
        _activeJihadC4Players[terrorist] = metadataCopy;
    }


}
