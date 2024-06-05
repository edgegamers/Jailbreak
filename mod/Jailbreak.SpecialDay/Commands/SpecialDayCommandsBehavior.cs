using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.SpecialDay.Commands;

public class SpecialDayCommandsBehavior() : IPluginBehavior
{
    private IWardenService _warden;
    
    private ISpecialDayMenu _menu;

    private ISpecialDayNotifications _specialDayNotifications;

    private SpecialDayHandler _handler;
    
    public SpecialDayCommandsBehavior(IWardenService warden, ISpecialDayMenu menu, ISpecialDayNotifications notifications, SpecialDayHandler handler) : this()
    {
        _warden = warden;
        _menu = menu;
        _specialDayNotifications = notifications;
        _handler = handler;
    }
    
    [ConsoleCommand("css_specialday", "Open the special day menu")]
    [ConsoleCommand("css_sd", "Open the special day menu")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Toggle(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        if (!_handler.CanStartSpecialDay())
        {
            _specialDayNotifications.SD_CANT_START.ToPlayerChat(player);
            return;
        }

        if (!_warden.IsWarden(player))
        {
            _specialDayNotifications.SD_NOT_WARDEN.ToPlayerChat(player);
            return;
        }

        _menu.OpenMenu(player);
    }
}
