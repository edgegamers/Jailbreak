using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.LastRequest;

namespace Jailbreak.English.LastRequest;

public class GunTossLocale : LastRequestLocale, ILRGunTossLocale {
  public IView PlayerThrewGunDistance(CCSPlayerController player, float dist) {
    return new SimpleView {
      { PREFIX, player, "threw their gun", dist, "units." }
    };
  }
}