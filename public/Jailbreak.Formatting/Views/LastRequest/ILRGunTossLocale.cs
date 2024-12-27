using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.LastRequest;

public interface ILRGunTossLocale {
  public IView PlayerThrewGunDistance(CCSPlayerController player, float dist);
}