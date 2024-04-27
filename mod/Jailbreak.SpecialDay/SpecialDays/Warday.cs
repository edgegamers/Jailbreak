using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Microsoft.VisualBasic.CompilerServices;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class Warday : ISpecialDay
{
    public string Name => "Warday";
    public string Description => "The guards can kill prisoners without any consequences.";

    private int timer = 0;
    private Timer timer1;
    private Timer timer2;
    private BasePlugin _plugin;

    public Warday(BasePlugin plugin)
    {
        _plugin = plugin;
    }
    
    public void OnStart()
    {
        var spawn = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").First();

        foreach (var player in Utilities.GetPlayers()
                     .Where(player => player.IsReal()))
        {
            player.Teleport(spawn.AbsOrigin);
            player.Freeze();
        }
        
        
    }

    private void AddTimers()
    {
        timer1 = _plugin.AddTimer(1f, () =>
        {
            foreach (var player in Utilities.GetPlayers()
                         .Where(player => player.IsReal()))
            {
                if (timer == 3 || player.Team == CsTeam.CounterTerrorist) player.UnFreeze();
                if (timer != 29 && player.Team != CsTeam.Terrorist) continue;
                
                player.UnFreeze();
                timer2.Kill();
            }
        }, TimerFlags.REPEAT);

        timer2 = _plugin.AddTimer(1f, () =>
        {
            timer++;
            if (timer == 29) timer1.Kill();
        }, TimerFlags.REPEAT);
    }

    public void OnEnd()
    {
        //do nothing for now
    }
    
}