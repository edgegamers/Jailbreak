using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public class CustomDay : ISpecialDay
{
    public string Name => "Freeday";
    public string Description => "All prisoners and guards are allowed to roam freely.";
    private readonly ISpecialDayNotifications _notifications;
    
    public CustomDay(BasePlugin plugin, ISpecialDayNotifications notifications)
    {
        _notifications = notifications;
    }
    
    public void OnStart()
    {
        _notifications.SD_CUSTOM_STARTED
            .ToAllChat()
            .ToAllCenter();
        
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()))
        {
            if (player.Team == CsTeam.Terrorist) FreezeManager.FreezePlayer(player, 10);
        }
    }
    
    
    public void OnEnd()
    {
        //do nothing for now
    }
}