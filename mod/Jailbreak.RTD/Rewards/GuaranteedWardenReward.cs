using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.RTD.Rewards;

public class GuaranteedWardenReward(IWardenSelectionService service)
  : IRTDReward {
  public string Name => "Guaranteed Warden";

  public string Description
    => "You are guaranteed to be warden next round if you queue for it";

  public bool CanGrantReward(CCSPlayerController player) {
    return player.Team == CsTeam.CounterTerrorist;
  }

  public bool GrantReward(CCSPlayerController player) {
    service.SetGuaranteedWarden(player);
    return true;
  }
}