using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Warden;
using System.Drawing;

using static Jailbreak.Public.Mod.Warden.IWardenLastGuardService;

namespace Jailbreak.Warden.Global;
/// <summary>
/// todo: Will probably have to use rebel service, warden service and others? 
/// </summary>
public class WardenLastGuardBehavior : IPluginBehavior, IWardenLastGuardService
{

    private readonly IWardenService _wardenService;
    private readonly IWardenLastGuardNotifications _wardenLastGuardNotifications;
    private readonly ICoroutines _coroutines;
    private BasePlugin? _parent;

    private int _numOfGuardsRoundStart;

    private bool _lastGuardEnabled;
    private int _lastGuardMaxHealth;
    private int _lastGuardRoundTimeSeconds;

    // todo add to a config file 
    public static readonly int _minNumberForLastGuard = 4;
    private static readonly string _lastGuardBeaconFile = "custom_content/particles/lastguard_beacon.vpcf";
    private List<CCSPlayerController> _activePlayerBeacons; // each player has a list of entities associated with them
    private int ticks = 0;
    private int step = 64 / 30; // per second 
    private bool waitTwoSeconds = false;


    public WardenLastGuardBehavior(IWardenService wardenService, IWardenLastGuardNotifications wardenLastGuardNotifications, ICoroutines coroutines)
    {
        _wardenService = wardenService;
        _wardenLastGuardNotifications = wardenLastGuardNotifications;
        _coroutines = coroutines;

        _numOfGuardsRoundStart = 0;

        _lastGuardEnabled = false;
        _lastGuardMaxHealth = 0;
        _lastGuardRoundTimeSeconds = 0;

        _activePlayerBeacons = new List<CCSPlayerController>();

    }

    public void Start(BasePlugin parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Enables the last guard functionality. There are no checks here to see if 
    /// there are more than x guards or checks for how many guards are alive. I have 
    /// done this intentionally in case for whatever reason you would want to activate
    /// last guard when these conditions aren't met. 
    /// 
    /// The warden recieves +75HP for each alive Terrorist at the time of last guard being activated.
    /// </summary>
    public void TryActivateLastGuard()
    {

        if (_lastGuardEnabled) { return; }
        if (_wardenService.Warden == null) { return; }
        CCSPlayerController wardenController = _wardenService.Warden;

        if (!_wardenService.HasWarden) { return; }
        if (!wardenController.IsValid) { return; }
        if (wardenController.PlayerPawn.Value == null) { return; }
        if (wardenController.CBodyComponent == null) { return; }
        if (wardenController.CBodyComponent.SceneNode == null) { return; }

        _lastGuardEnabled = true; // only enable last guard after we've set everything we need
        _activePlayerBeacons.Add(wardenController);

        _lastGuardMaxHealth = 0; // default health of warden
        _lastGuardRoundTimeSeconds = 0;
        IterateThroughTeam((terroristPlayer) =>
        {
            if (!terroristPlayer.PawnIsAlive) { return; }

            // for each Terrorist give the Last Guard +75HP and +10s to kill the remaining Prisoners
            _lastGuardMaxHealth += 75;
            _lastGuardRoundTimeSeconds += 10;
            _activePlayerBeacons.Add(terroristPlayer);

        }, CsTeam.Terrorist);

        // funny way of checking if there are 2 terrorists or less when trying to invoke last guard initially
        //if (_lastGuardRoundTimeSeconds <= 20) { return; } // todo UNCOMMENT IT'S JUST FOR MY OWN TESTING

        // again counts if there are 6 or less Prisoners for the last guard, if so then set some BASE attributes for the last guard
        if (_lastGuardRoundTimeSeconds < 60) { _lastGuardRoundTimeSeconds = 60; _lastGuardMaxHealth = 400; }
        _lastGuardMaxHealth /= 2; // important otherwise it's wayyy to OP :)

        // I don't like having an odd health...
        if (_lastGuardMaxHealth % 2 != 0) {  _lastGuardMaxHealth += 5; } 

        /**
         * Whenever you change the state of an entity you must tell the server.
         * The server then updates the individual clients.
         * This is because all the clients share the same state as the server.
         * In CS# when you change the state of a networked entity, NetworkStateChanged() is not automatically called,
         * therefore we must use Utilities.SetStateChanged()
         */

        /**
         * Another interesting thing is that there are server-side entities and client-side entities, and "common" entities...
         * To quote Poggu "It's a way to network classes to clients; the game can't really do that outside entities".
         * https://developer.valvesoftware.com/wiki/Networking_Entities
         */

        // set the player's Pawn health and tell the server about it
        wardenController.PlayerPawn.Value.Health = _lastGuardMaxHealth;
        wardenController.PlayerPawn.Value.ArmorValue = 125; // in line with how it used to work
        Utilities.SetStateChanged(wardenController.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
        Utilities.SetStateChanged(wardenController.PlayerPawn.Value, "CCSPlayerPawnBase", "m_ArmorValue");

        // set the round time to the last guard round time and tell the server about it
        CCSGameRulesProxy serverRulesEntity = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First();

        CCSGameRules serverGameRules = serverRulesEntity.GameRules!;
        serverGameRules.RoundTime = _lastGuardRoundTimeSeconds;

        Utilities.SetStateChanged(serverRulesEntity, "CCSGameRulesProxy", "m_pGameRules");

        _wardenLastGuardNotifications.LASTGUARD_ACTIVATED(wardenController.PlayerName).ToAllChat().ToAllCenter();
        _wardenLastGuardNotifications.LASTGUARD_MAXHEALTH(_lastGuardMaxHealth).ToAllChat().ToAllCenter();
        _wardenLastGuardNotifications.LASTGUARD_TIMELIMIT(_lastGuardRoundTimeSeconds).ToAllChat().ToAllCenter();

    }

    public void TryDeactivateLastGuard()
    {
        _lastGuardEnabled = false;
        _activePlayerBeacons.Clear();

         // remove all colours from player models, all beacons
         // set time to 30 seconds (for LR)


    }

    public bool LastGuardPossible()
    {
        return (_numOfGuardsRoundStart >= _minNumberForLastGuard);
    }


    [ConsoleCommand("css_test", "Pass warden onto another player")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Test(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;


        TryActivateLastGuard();

    }

    // get player pos:
    // Vector pos = player.PlayerPawn.Value!.AbsOrigin!;

    // PLEASE PLEASE REMOVE KLJASDKLJSAKLJDSAKLJDKLJ
    [ConsoleCommand("css_d", "")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Debug(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;


        float width = 1.0f;
        float radius = 10.0f;
        if (command.ArgCount > 1)
        {
            width = Convert.ToSingle(command.GetArg(1));
        }
        if (command.ArgCount > 2)
        {
            radius = Convert.ToSingle(command.GetArg(2));
        }


        player.PrintToChat($"Width: {width}");

    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {

        _numOfGuardsRoundStart = 0;
        IterateThroughTeam( (guardPlayer) => _numOfGuardsRoundStart += 1, CsTeam.CounterTerrorist);

        return HookResult.Continue; 

    }

    [GameEventHandler]
    public HookResult OnPlayerLeave(EventPlayerDisconnect @event, GameEventInfo info)
    {

        if (!_lastGuardEnabled) { return HookResult.Continue; }

        int numOfTerrorists = 0;
        IterateThroughTeam((terroristPlayer) =>
        {
            numOfTerrorists++;
        }, CsTeam.Terrorist);

        if (numOfTerrorists <= 2)
        {
            TryDeactivateLastGuard();
        }

        return HookResult.Continue;

    }


    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundStart @event, GameEventInfo info)
    {
    
        if (_lastGuardEnabled) { TryDeactivateLastGuard(); }
        // todo unset everyone's player model colour back to default (255?)

        return HookResult.Continue;

    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {

        if (_lastGuardEnabled)
        {

            int numOfAliveTerrorists = 0;
            IterateThroughTeam( (terroristPlayer) =>
            {
                numOfAliveTerrorists += 1;
            }, CsTeam.Terrorist);

            if (numOfAliveTerrorists <= 2)
                TryDeactivateLastGuard();

            return HookResult.Continue;

        }
        else
        {
            int numOfAliveGuards = 0;
            IterateThroughTeam( (guardPlayer) =>
            {
                if (guardPlayer.PawnIsAlive) { numOfAliveGuards += 1; }
            }, CsTeam.CounterTerrorist);

            if (numOfAliveGuards != 1) { return HookResult.Continue; }
            if (!LastGuardPossible()) { return HookResult.Continue; } // todo maybe tell the server last guard ain't possible.

            // else last guard is most definitely possible :D 
            TryActivateLastGuard();

            return HookResult.Continue;
        }

    }

    /// <summary>
    /// Iterates through the specified team and invokes the callback only if the player in question is VALID
    /// </summary>
    /// <param name="callback">The PlayerHandle WILL be valid.</param>
    /// <param name="team"></param>
    public void IterateThroughTeam(GuardCallback callback, CsTeam team)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!player.IsValid) {  continue; }
            if (player.GetTeam() == team)
            {
                callback.Invoke(player);
            }
        }
    }

}
