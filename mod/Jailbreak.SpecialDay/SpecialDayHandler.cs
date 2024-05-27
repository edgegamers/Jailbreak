using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayHandler : ISpecialDayHandler, IPluginBehavior
{
    private int _roundsSinceLastSpecialDay = 0;
    private bool _isSpecialDayActive = false;
    private ISpecialDay? _currentSpecialDay = null;
    private BasePlugin _plugin;

    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
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
    
    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _roundsSinceLastSpecialDay++;
        return HookResult.Continue;
    }
    
    public int RoundsSinceLastSpecialDay()
    {
        return _roundsSinceLastSpecialDay;
    }

    public bool CanStartSpecialDay()
    {
        return RoundsSinceLastSpecialDay() >= 2;
    }

    public bool IsSpecialDayActive()
    {
        return _isSpecialDayActive;
    }

    public bool StartSpecialDay(string name)
    {
        if (_isSpecialDayActive || !CanStartSpecialDay()) return false;
        
        var fullName = "Jailbreak.SpecialDay.SpecialDays";
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == fullName && t.GetInterface("ISpecialDay") != null
            select t;

        foreach (var type in q)
        {
            if (type == null) continue;
            var item = (ISpecialDay) Activator.CreateInstance(type, _plugin);
            if (item == null) continue;
            if (item.Name != name) continue;
            
            _currentSpecialDay = item;
            _isSpecialDayActive = true;
            _currentSpecialDay.OnStart();
            break;
        }
        
        Server.NextFrame(() => Server.PrintToChatAll($"{_currentSpecialDay?.Name} has started - {_currentSpecialDay?.Description}"));
        return true;
    }
}