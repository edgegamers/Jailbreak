using System.Collections;
using System.Drawing;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.RTD.Rewards;

namespace Jailbreak.RTD;

public class RewardGenerator(IZoneManager mgr, IC4Service bomb)
  : IPluginBehavior, IRewardGenerator {
  private static readonly float PROB_LOTTERY = 1 / 5000f;
  private static readonly float PROB_EXTREMELY_LOW = 1 / 1000f;
  private static readonly float PROB_VERY_LOW = 1 / 100f;
  private static readonly float PROB_LOW = 1 / 20f;
  private static readonly float PROB_MEDIUM = 1 / 10f;
  private static readonly float PROB_OFTEN = 1 / 5f;
  private static readonly float PROB_VERY_OFTEN = 1 / 2f;

  private readonly List<(IRTDReward, float)> rewards = [
    (new NothingReward(), PROB_OFTEN),
    (new WeaponReward("weapon_healthshot"), PROB_OFTEN),
    (new WeaponReward("weapon_decoy"), PROB_OFTEN),
    (new CreditReward(1), PROB_MEDIUM), (new CreditReward(2), PROB_MEDIUM),
    (new CreditReward(3), PROB_MEDIUM),
    (new WeaponReward("weapon_flashbang"), PROB_MEDIUM),
    (new WeaponReward("weapon_hegrenade"), PROB_MEDIUM),
    (new WeaponReward("weapon_smokegrenade"), PROB_MEDIUM),
    (new WeaponReward("weapon_molotov"), PROB_MEDIUM),
    (new WeaponReward("weapon_taser"), PROB_MEDIUM),
    (new HPReward(150), PROB_MEDIUM), (new HPReward(50), PROB_MEDIUM),
    (new ArmorReward(150), PROB_MEDIUM), (new HPReward(1), PROB_LOW),
    (new ColorReward(Color.FromArgb(0, 255, 0), true), PROB_LOW),
    (new ColorReward(Color.FromArgb(255, 0, 0), true), PROB_LOW),
    (new TransparentReward(), PROB_LOW),
    (new AmmoWeaponReward("weapon_glock", 2, 0), PROB_LOW),
    (new AmmoWeaponReward("weapon_negev", 0, 5), PROB_LOW),
    (new NoWeaponReward(), PROB_VERY_LOW),
    (new AmmoWeaponReward("weapon_deagle", 1, 0), PROB_VERY_LOW),
    (new RandomTeleportReward(mgr), PROB_VERY_LOW),
    (new BombReward(bomb), PROB_VERY_LOW),
    (new AmmoWeaponReward("weapon_awp", 1, 0), PROB_EXTREMELY_LOW)
  ];

  private readonly Random rng = new();

  private float totalWeight
    => rewards.Where(r => r.Item1.Enabled).Select(s => s.Item2).Sum();

  public void Start(BasePlugin basePlugin) {
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.GRENADE),
      PROB_LOW));
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.UTILITY),
      PROB_LOW));
    rewards.Add((new CannotScopeReward(basePlugin), PROB_LOW));
    rewards.Add((
      new CannotPickupReward(basePlugin,
        WeaponType.GRENADE | WeaponType.UTILITY), PROB_LOW));
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.PISTOLS),
      PROB_VERY_LOW));
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.SNIPERS),
      PROB_VERY_LOW));
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.HEAVY),
      PROB_VERY_LOW));
    rewards.Add((new CannotPickupReward(basePlugin, WeaponType.RIFLES),
      PROB_EXTREMELY_LOW));
  }

  public IRTDReward GenerateReward(int? id) {
    var roll = rng.NextDouble() * totalWeight;

    foreach (var reward in rewards.Where(reward => reward.Item1.Enabled)) {
      if (id != null && reward.Item1.CanGrantReward(id.Value)) continue;
      roll -= reward.Item2;
      if (roll <= 0) return reward.Item1;
    }

    throw new InvalidOperationException("No reward was generated.");
  }

  public IEnumerator<(IRTDReward, float)> GetEnumerator() {
    return rewards.Where(r => r.Item1.Enabled).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
  public int Count => rewards.Count(r => r.Item1.Enabled);
}