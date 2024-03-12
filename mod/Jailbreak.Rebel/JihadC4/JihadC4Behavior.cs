using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Mod.Rebel;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Entities;

namespace Jailbreak.Rebel.JihadC4;

public class JihadC4Behavior : IPluginBehavior, IJihadC4Service
{

    // use services we want here

    // any entry in this list implies the player has an active jihad c4 in their pawn inventory
    private List<CCSPlayerController> _activeJihadC4Players;


    public JihadC4Behavior()
    {
        _activeJihadC4Players = new List<CCSPlayerController>();
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
        if (bombEntity.Entity == null || bombEntity.Entity.Name == null || !bombEntity.Entity.Name.Equals("jihad_c4")) { return HookResult.Continue; } 
        if (!_activeJihadC4Players.Contains(@event.Userid)) { return HookResult.Continue; }

        // else the JihadC4 is active for the specified player and we can invoke TryBlowUpJihadC4()
        
        TryBlowUpJihadC4(@event.Userid); // todo this is not being called when the jihad c4 is held out ...

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnTerroristEquipJihadC4(EventItemEquip @event, GameEventInfo info)
    {
        Console.WriteLine("bro is holding out the C4 but the function isn't going down");
        if (@event.Userid == null || !@event.Userid.IsValid) { return HookResult.Continue; }
        if (@event.Weptype != ((uint)CSWeaponType.WEAPONTYPE_C4)) { return HookResult.Continue; }
        if (!_activeJihadC4Players.Contains(@event.Userid)) { return HookResult.Continue; }

        // else the currently held out item is a weapon_c4 and the player holding it has the capabilities to blow it up

        // todo display info about jihad c4
        Console.WriteLine("wtf4"); 
        
        // why is this not printing but the console writeline is? :)
        @event.Userid.PrintToChat("You can set a delay with !delay <seconds>");

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

        _activeJihadC4Players.Add(terrorist);

        // todo add description of what you've just recieved
        terrorist.PrintToChat("You've recieved the Jihad C4. Hold it out and press G to detonate it.");

    }


    // hmm todo :)
    public void TryBlowUpJihadC4(CCSPlayerController terrorist, Vector location)
    {
        Console.WriteLine("holy shit bro just tried to blow up the ACTIVE c4");
        // we 
    }

    public void TryDefuseJihadC4(CCSPlayerController terrorist)
    {
        throw new NotImplementedException();
    }

    public void TrySetDelayJihadC4(CCSPlayerController terrorist, float delay)
    {
        throw new NotImplementedException();
    }


}
