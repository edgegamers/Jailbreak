using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class FakeBombReward() : WeaponReward("weapon_c4") {
  public override string Name => "Fake Bomb";

  public override string Description
    => "You will spawn with a fake bomb next round.";
}