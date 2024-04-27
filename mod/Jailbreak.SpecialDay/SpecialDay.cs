using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDay : ISpecialDayHandler, IPluginBehavior
{
    private int _roundsSinceLastSpecialDay = 0;
    private bool _isSpecialDayActive = false;
    private ISpecialDay? _currentSpecialDay = null;

    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);

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

    public void StartSpecialDay(string name)
    {
        if (_isSpecialDayActive || !CanStartSpecialDay()) return;
        
        var fullName = "Jailbreak.SpecialDay.SpecialDays";
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == fullName && t.GetInterface("ISpecialDay") != null
            select t;

        foreach (var type in q)
        {
            if (type == null) return;
            var item = (ISpecialDay) Activator.CreateInstance(type);
            if (item == null) return;
            if (item.Name != name) continue;
            
            _currentSpecialDay = item;
            _isSpecialDayActive = true;
            _currentSpecialDay.OnStart();
            break;
        }
    }
}