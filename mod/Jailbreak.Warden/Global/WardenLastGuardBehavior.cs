using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using static Jailbreak.Public.Mod.Warden.IWardenLastGuardService;

namespace Jailbreak.Warden.Global;
/// <summary>
/// Will probably have to use rebel service, warden service and others? 
/// </summary>
public class WardenLastGuardBehavior : IPluginBehavior, IWardenLastGuardService
{

    private readonly IWardenService _wardenService;
    private readonly IWardenLastGuardNotifications _wardenLastGuardNotifications;

    private bool _lastGuardEnabled;
    private int _numOfGuardsRoundStart;
    private int _lastGuardMaxHealth;

    // todo add to a config file 
    public static readonly int _minNumberForLastGuard = 4;



    public WardenLastGuardBehavior(IWardenService wardenService, IWardenLastGuardNotifications wardenLastGuardNotifications)
    {
        _wardenService = wardenService;
        _wardenLastGuardNotifications = wardenLastGuardNotifications;

        _lastGuardEnabled = false;
        _numOfGuardsRoundStart = 0;
        _lastGuardMaxHealth = 0;

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

        if (!_wardenService.HasWarden) { return; }
        if (_wardenService.Warden == null) { return; }

        _lastGuardEnabled = true;

        int totalTerroristHealth = 0; // default health of warden
        int totalLastGuardSeconds = 0;
        IterateThroughTeam((terroristPlayer) =>
        {
            if (!terroristPlayer.PawnIsAlive) { return; }

            totalTerroristHealth += 75; // a deliberate number
            totalLastGuardSeconds += 10;
            // for 10 players this gives warden HP as 

        }, CsTeam.Terrorist);

        totalTerroristHealth /= 2;

        // I don't like having an odd health... 
        if (totalTerroristHealth % 2 != 0)
        {
            totalTerroristHealth += 5;
        }

        // TODO PLS REMOVE
        totalTerroristHealth = 1000;

        // being extra careful
        if (!_wardenService.Warden.IsValid) { return; } 
        if (_wardenService.Warden.PlayerPawn.Value == null) { return; }

        _lastGuardMaxHealth = totalTerroristHealth;
        _wardenService.Warden.PlayerPawn.Value.Health = _lastGuardMaxHealth;

        // todo see if this works?
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        gameRules.RoundTime = 100;

        // if health is in the 1000s trunate the last 2 digits

        // TODO CALCULATE TIME LIMIT
        int lastGuardTimeLimitSeconds = 60;
        _wardenLastGuardNotifications.LASTGUARD_ACTIVATED(_wardenService.Warden.PlayerName).ToAllChat().ToAllCenter();
        _wardenLastGuardNotifications.LASTGUARD_MAXHEALTH(_lastGuardMaxHealth).ToAllChat().ToAllCenter();
        _wardenLastGuardNotifications.LASTGUARD_TIMELIMIT(lastGuardTimeLimitSeconds).ToAllChat().ToAllCenter();

    }

    public void TryDeactivateLastGuard()
    {
        _lastGuardEnabled = false;

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

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {

        _numOfGuardsRoundStart = 0;
        IterateThroughTeam( (guardPlayer) => _numOfGuardsRoundStart += 1, CsTeam.CounterTerrorist);

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

            if (numOfAliveTerrorists == 2)
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
