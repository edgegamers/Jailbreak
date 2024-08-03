using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.LastRequest;

namespace Jailbreak.English.LastRequest;

public class B4BLocale : LastRequestLocale, ILRB4BLocale {
  public IView PlayerGoesFirst(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, "Randomly selected ", player.PlayerName, "to go first."
    };
  }

  public IView WeaponSelected(CCSPlayerController player, string weapon) {
    return new SimpleView { PREFIX, player, "picked", weapon };
  }
}