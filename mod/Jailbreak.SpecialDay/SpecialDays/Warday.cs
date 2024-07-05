using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Damage;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class Warday : ISpecialDay, IBlockUserDamage
{
    public string Name => "Warday";
    public string Description => $" {ChatColors.Red}[Warday] {ChatColors.Blue} Guards versus Prisoners. Your goal is to ensure that your team is last team standing!";
    public bool ShouldJihadC4BeEnabled => false;
    private int timer = 0;
    private Timer timer1;
    private BasePlugin _plugin;
    private bool _hasStarted = true;
    private readonly ISpecialDayNotifications _notifications;

    public Warday(BasePlugin plugin, ISpecialDayNotifications notifications)
    {
        _notifications = notifications;
        _plugin = plugin;
    }

    public bool ShouldBlockDamage(CCSPlayerController player, CCSPlayerController? attacker, EventPlayerHurt @event)
    {
        if (_hasStarted)
        {
            return false;
        }
        return true;
    }

    public void OnStart()
    {
        _notifications.SD_WARDAY_STARTED
            .ToAllChat()
            .ToAllCenter();
            
        var spawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();

        foreach (var player in Utilities.GetPlayers()
                     .Where(player => player.IsReal()))
        {
            var max = spawns.Count;

            var index = new Random().Next(0, max);
            
            player.PlayerPawn.Value!.Teleport(spawns[index].AbsOrigin);
            if (player.Team == CsTeam.Terrorist) FreezeManager.FreezePlayer(player, 30);
        }
        _hasStarted = false;
        AddTimers();
    }

    private void AddTimers()
    {
        timer1 = _plugin.AddTimer(1f, () =>
        {
            timer++;

            if (timer != 30) return;
            _hasStarted = true;
            timer1.Kill();
        }, TimerFlags.REPEAT);

    }

    public void OnEnd()
    {
        //do nothing for now
    }

}
