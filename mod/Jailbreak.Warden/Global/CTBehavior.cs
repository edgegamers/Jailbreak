using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Warden.Global;

public class CTBehavior(WardenConfig config) : IPluginBehavior
{
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
    {
        var armor = getBalance() switch
        {
            0 => 50, // Balanced teams
            1 => 100, // Ts outnumber CTs
            -1 => 25, // CTs outnumber Ts
            _ => 50
        };
        foreach (var player in Utilities.GetPlayers()
                     .Where(p => p.IsReal() && p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true }))
        {
            player.PawnArmor = armor;
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// Checks if CTs outnumber Ts or vice versa
    /// </summary>
    /// <returns>1 Ts outnumber CTs, 0 if even, and -1 if CTs outnumber Ts</returns>
    private int getBalance()
    {
        var ctCount = Utilities.GetPlayers().Count(p => p.IsReal() && p.Team == CsTeam.CounterTerrorist);
        var tCount = Utilities.GetPlayers().Count(p => p.IsReal() && p.Team == CsTeam.Terrorist);

        var ratio = (float)tCount / config.TerroristRatio - ctCount;

        return ratio switch
        {
            > 0 => 1,
            0 => 0,
            _ => -1
        };
    }
}