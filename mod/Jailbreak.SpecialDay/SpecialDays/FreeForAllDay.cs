using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
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

public class FreeForAllDay : ISpecialDay, IBlockUserDamage
{
    public string Name => "Warday";
    public string Description => "Everyone for themselves. Your goal is to be the last man standing!";

    private int timer = 0;
    private Timer? timer1;
    private BasePlugin _plugin;
    private bool _hasStarted = true;
    private readonly ISpecialDayNotifications _notifications;

    public FreeForAllDay(BasePlugin plugin, ISpecialDayNotifications notifications)
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
        _notifications.SD_FFA_STARTED
            .ToAllChat()
            .ToAllCenter();
        var spawn = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();

        foreach (var player in Utilities.GetPlayers()
                     .Where(player => player.IsReal()))
        {
            var max = spawn.Count;

            var index = new Random().Next(0, max);

            player.PlayerPawn.Value!.Teleport(spawn[index].AbsOrigin);
            FreezeManager.FreezePlayer(player, 3);
        }

        var friendlyFire = ConVar.Find("mp_friendlyfire");
        var teammates = ConVar.Find("mp_teammates_are_enemies");

        if (friendlyFire == null) return;

        var friendlyFireValue = friendlyFire.GetPrimitiveValue<bool>(); //assume false in this example, use GetNativeValue for vectors, Qangles, etc

        if (!friendlyFireValue)
        {
            friendlyFire.SetValue<bool>(true);
        }

        if (teammates == null) return;

        teammates.SetValue<bool>(true);

        _hasStarted = false;
        AddTimers();
    }

    private void AddTimers()
    {
        timer1 = _plugin.AddTimer(1f, () =>
        {
            timer++;

            if (timer != 15) return;

            _hasStarted = true;

            timer1.Kill();
        }, TimerFlags.REPEAT);

    }

    public void OnEnd()
    {

        var friendlyFire = ConVar.Find("mp_friendlyfire");
        var teammates = ConVar.Find("mp_teammates_are_enemies");

        if (friendlyFire == null) return;

        var friendlyFireValue = friendlyFire.GetPrimitiveValue<bool>(); //assume false in this example, use GetNativeValue for vectors, Qangles, etc

        if (friendlyFireValue)
        {
            friendlyFire?.SetValue<bool>(false);
        }

        teammates?.SetValue<bool>(false);

    }

}
