using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class NoWeaponReward : IRTDReward {
  public string Name => "No Weapon";
  public string Description => "You will not spawn with a knife next round.";

  public bool GrantReward(CCSPlayerController player) {
    foreach (var offset in (int[]) [64, 128, 256]) {
      Server.RunOnTick(Server.TickCount + offset, () => {
        if (!player.IsValid) return;
        player.RemoveWeapons();
      });
    }

    return true;
  }
}