using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Damage;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayHandler(SpecialDayConfig config, IJihadC4Service jihadC4Service) : ISpecialDayHandler, IPluginBehavior
{
    private int _roundsSinceLastSpecialDay = 0;
    private bool _isSpecialDayActive = false;
    private int _roundStartTime = 0;
    private ISpecialDay? _currentSpecialDay = null;
    private BasePlugin _plugin;

    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        plugin.RegisterEventHandler<EventPlayerHurt>(DamageHandler);
        _plugin = plugin;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!_isSpecialDayActive && _currentSpecialDay == null) return HookResult.Continue;

        _isSpecialDayActive = false;
        _currentSpecialDay?.OnEnd();

        _currentSpecialDay = null;
        _roundsSinceLastSpecialDay = 0;

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    private HookResult DamageHandler(EventPlayerHurt @event, GameEventInfo info)
    {
        if (_currentSpecialDay == null)
        {
            return HookResult.Continue;
        }
        if (_currentSpecialDay is IBlockUserDamage damageHandler)
        {
            return damageHandler.BlockUserDamage(@event, info);
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _roundStartTime = (int)Math.Round(Server.CurrentTime);
        if (ServerExtensions.GetGameRules().WarmupPeriod)
            return HookResult.Continue;
        _roundsSinceLastSpecialDay++;
        return HookResult.Continue;
    }

    public int RoundsSinceLastSpecialDay()
    {
        return _roundsSinceLastSpecialDay;
    }

    public bool CanStartSpecialDay()
    {
        return RoundsSinceLastSpecialDay() >= config.MinRoundsBeforeSpecialDay && ((int)Math.Round(Server.CurrentTime - _roundStartTime)) <= config.MaxRoundSecondsBeforeSpecialDay;
    }

    public bool IsSpecialDayActive()
    {
        return _isSpecialDayActive;
    }

    public bool StartSpecialDay<ISpecialDayNotifications>(string name, ISpecialDayNotifications _notifications)
    {
        if (_isSpecialDayActive || !CanStartSpecialDay()) return false;

        var fullName = "Jailbreak.SpecialDay.SpecialDays";
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == fullName && t.GetInterface("ISpecialDay") != null
                select t;

        foreach (var type in q)
        {
            if (type == null) continue;
            var item = (ISpecialDay)Activator.CreateInstance(type, _plugin, _notifications);
            if (item == null) continue;
            if (item.Name != name) continue;

            if (!item.ShouldJihadC4BeEnabled)
            {
                jihadC4Service.ClearActiveC4s();
            }

            _currentSpecialDay = item;
            _isSpecialDayActive = true;
            _currentSpecialDay.OnStart();
            break;
        }

        //Server.NextFrame(() => Server.PrintToChatAll($"{_currentSpecialDay?.Name} has started - {_currentSpecialDay?.Description}"));
        return true;
    }
}