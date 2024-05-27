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

    private IWardenNotifications _notifications;

    private SpecialDayHandler _handler;
    
    public SpecialDayCommandsBehavior(IWardenService warden, ISpecialDayMenu menu, IWardenNotifications notifications, SpecialDayHandler handler) : this()
    {
        _warden = warden;
        _menu = menu;
        _notifications = notifications;
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
            player.PrintToChat("Special Day has already happened recently!");
        }

        if (!_warden.IsWarden(player))
        {
            _notifications.NOT_WARDEN.ToPlayerChat(player);
            //	You're not that warden, blud
            return;
        }

        _menu.OpenMenu(player);
    }
}
