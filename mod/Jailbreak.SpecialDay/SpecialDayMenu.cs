using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayMenu : ISpecialDayMenu, IPluginBehavior
{
    private BaseMenu _menu;
    private readonly ISpecialDayHandler _handler;
    private readonly BasePlugin _plugin;

    public SpecialDayMenu(BasePlugin plugin, ISpecialDayHandler handler)
    {
        _menu = new CenterHtmlMenu("Special Days", plugin);
        _plugin = plugin;
        _handler = handler;
        AddSpecialDays();
    }

    private void AddSpecialDays()
    {
        var fullName = "Jailbreak.SpecialDay.SpecialDays";
        var types = from type in Assembly.GetExecutingAssembly().GetTypes()
            where type.IsClass && type.Namespace == fullName && type.GetInterface("ISpecialDay") != null
            select type;

        foreach (var type in types)
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
            MenuManager.CloseActiveMenu(player);
        });
    }
    
    public void OpenMenu(CCSPlayerController player)
    {
        _menu.Open(player);
    }
}