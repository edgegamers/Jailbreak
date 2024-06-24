using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jailbreak.Formatting.Views;

public interface IBombNotifications
{
    public IView DETONATING_BOMB(CCSPlayerController player);

    public IView PLAYER_RESULTS(int damage, int kills);

    public IView BOMB_DROPPED { get; }
    public IView BOMB_PICKUP { get; }
    public IView BOMB_RECEIVED { get; }
    public IView BOMB_USAGE { get; }


}
