using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ILastGuardNotifications {
  public IView LGStarted(CCSPlayerController lastGuard, int ctHealth,
    int tHealth);
}