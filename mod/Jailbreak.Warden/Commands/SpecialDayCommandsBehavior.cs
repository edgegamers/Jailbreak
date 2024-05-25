using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class SpecialDayCommandsBehavior : IPluginBehavior
{
    private IWardenService _warden;
    
    private ISpecialDayMenu _menu;
    
    public SpecialDayCommandsBehavior(IWardenService warden, ISpecialDayMenu menu)
    {
        _warden = warden;
        _menu = menu;
    }
    
    [ConsoleCommand("css_specialday", "Open the special day menu")]
    [ConsoleCommand("css_sd", "Open the special day menu")]
    [CommandHelper(1, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Toggle(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
    
        if (!_warden.IsWarden(player))
            //	You're not that warden, blud
            return;

        _menu.OpenMenu(player);
    }
}