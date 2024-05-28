using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Microsoft.VisualBasic.CompilerServices;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class Warday : ISpecialDay
{
    public string Name => "Warday";
    public string Description => "Guards versus Prisoners. Your goal is to ensure that your team is last team standing!";

    private int timer = 0;
    private Timer timer1;
    private BasePlugin _plugin;
    private bool _hasStarted = true;
    private readonly ISpecialDayNotifications _notifications;

    public Warday(BasePlugin plugin)
    {
        _plugin = plugin;
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(_ => _hasStarted ? HookResult.Continue : HookResult.Stop, HookMode.Pre);    }
    
    public void OnStart()
    {
        _notifications.SD_WARDAY_STARTED
            .ToAllChat()
            .ToAllCenter();
        var spawn = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").First();

        foreach (var player in Utilities.GetPlayers()
                     .Where(player => player.IsReal()))
        {
            player.PlayerPawn.Value!.Teleport(spawn.AbsOrigin);
            player.Freeze();
        }
        _hasStarted = false;
        AddTimers();
    }

    private void AddTimers()
    {
        timer1 = _plugin.AddTimer(1f, () =>
        {
            timer++;
            foreach (var player in Utilities.GetPlayers()
                         .Where(player => player.IsReal()))
            {
                if (timer == 3 || player.Team == CsTeam.CounterTerrorist) player.UnFreeze();
                if (timer != 29 && player.Team != CsTeam.Terrorist) continue;
                
                player.UnFreeze();
                _hasStarted = true;
                timer1.Kill();
                
            }
        }, TimerFlags.REPEAT);
        
    }

    public void OnEnd()
    {
        //do nothing for now
    }
    
}