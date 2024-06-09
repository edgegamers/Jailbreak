using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayMenu : ISpecialDayMenu, IPluginBehavior
{
    private BaseMenu _menu;
    private readonly ISpecialDayHandler _handler;
    private BasePlugin _plugin;
    private readonly ISpecialDayNotifications _notifications;

    public SpecialDayMenu(ISpecialDayHandler handler, ISpecialDayNotifications notifications)
    {
        _notifications = notifications;
        _handler = handler;
    }

    public void Start(BasePlugin plugin) {
        _menu = new CenterHtmlMenu("Special Days", plugin);
        _plugin = plugin;
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
            var item = (ISpecialDay?) Activator.CreateInstance(type, _plugin, _notifications);
            if (item == null) return;
            AddSpecialDay(item);
        }
    }
    
    private void AddSpecialDay(ISpecialDay specialDay)
    {
        _menu.AddMenuOption(specialDay.Name, (player, _menu) =>
        {
            _handler.StartSpecialDay(specialDay.Name, _notifications);
            MenuManager.CloseActiveMenu(player);
        });
    }
    
    public void OpenMenu(CCSPlayerController player)
    {
        _menu.Open(player);
    }
}