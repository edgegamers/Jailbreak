using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayMenu : ISpecialDayMenu
{
    private BaseMenu _menu;
    private readonly ISpecialDayHandler _handler;
    private readonly BasePlugin _plugin;

    public SpecialDayMenu(BasePlugin plugin, SpecialDayHandler handler)
    {
        _menu = new CenterHtmlMenu("Special Days", plugin);
        _plugin = plugin;
        _handler = handler;
        AddSpecialDays();
    }

    private void AddSpecialDays()
    {
        var fullName = "Jailbreak.SpecialDay.SpecialDays";
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == fullName && t.GetInterface("ISpecialDay") != null
            select t;

        foreach (var type in q)
        {
            if (type == null) return;
            var item = (ISpecialDay?) Activator.CreateInstance(type, _plugin);
            if (item == null) return;
            AddSpecialDay(item);
        }
    }
    
    private void AddSpecialDay(ISpecialDay specialDay)
    {
        _menu.AddMenuOption(specialDay.Name + " - " + specialDay.Description, (player, _menu) =>
        {
            _handler.StartSpecialDay(specialDay.Name);
        });
    }
    
    public void OpenMenu(CCSPlayerController player)
    {
        _menu.Open(player);
    }
}