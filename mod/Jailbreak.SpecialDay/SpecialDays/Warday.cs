using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Microsoft.VisualBasic.CompilerServices;

namespace Jailbreak.SpecialDay.SpecialDays;

public class Warday : ISpecialDay
{
    public string Name => "Warday";

    public string Description => "The guards can kill prisoners without any consequences.";

    public void OnStart()
    {
        var spawn = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").First();

        foreach (var player in Utilities.GetPlayers()
                     .Where(player => player.IsReal() && player.Team == CsTeam.Terrorist))
        {
            player.Teleport(spawn.AbsOrigin);
        }
    }

    public void OnEnd()
    {
        //do nothing for now
    }
}