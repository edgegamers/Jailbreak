using System.Collections;
using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Views.RTD;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.RTD.Rewards;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.RTD;

public class RewardGenerator(IC4Service bomb, IWardenSelectionService warden,
  IRTDLocale locale, IServiceProvider provider)
  : IPluginBehavior, IRewardGenerator {
  private const float PROB_LOTTERY = 1 / 5000f;
  private const float PROB_EXTREMELY_LOW = 1 / 800f;
  private const float PROB_VERY_LOW = 1 / 100f;
  private const float PROB_LOW = 1 / 20f;
  private const float PROB_MEDIUM = 1 / 10f;
  private const float PROB_OFTEN = 1 / 5f;
  private const float PROB_VERY_OFTEN = 1 / 2f;

  private readonly List<(IRTDReward, float)> rewards = [];

  private readonly Random rng = new();

  private float totalWeight
    => rewards.Where(r => r.Item1.Enabled).Select(s => s.Item2).Sum();

  public void Start(BasePlugin basePlugin) {
    rewards.AddRange([
      // Very often
      (new NothingReward(), PROB_VERY_OFTEN),
      (new CreditReward(1, locale), PROB_VERY_OFTEN),
      (new CreditReward(-1, locale), PROB_VERY_OFTEN),

      // Often
      (new WeaponReward("weapon_healthshot"), PROB_OFTEN),
      (new WeaponReward("weapon_decoy"), PROB_OFTEN),
      (new HPReward(110), PROB_OFTEN), (new ArmorReward(15), PROB_OFTEN),
      (new CreditReward(-10, locale), PROB_VERY_OFTEN),
      (new CreditReward(5, locale), PROB_MEDIUM),

      // Medium
      (new CreditReward(10, locale), PROB_MEDIUM),
      (new CreditReward(25, locale), PROB_MEDIUM),
      (new WeaponReward("weapon_flashbang"), PROB_MEDIUM),
      (new WeaponReward("weapon_hegrenade"), PROB_MEDIUM),
      (new WeaponReward("weapon_smokegrenade"), PROB_MEDIUM),
      (new WeaponReward("weapon_molotov"), PROB_MEDIUM),
      (new WeaponReward("weapon_taser"), PROB_MEDIUM),
      (new CannotUseReward(basePlugin, WeaponType.UTILITY), PROB_MEDIUM),
      (new HPReward(150), PROB_MEDIUM), (new HPReward(50), PROB_MEDIUM),
      (new ArmorReward(150), PROB_MEDIUM),
      (new GuaranteedWardenReward(warden), PROB_MEDIUM),
      (new WeaponReward("weapon_g3sg1", CsTeam.CounterTerrorist),
        PROB_MEDIUM / 2),

      // Low
      (new AmmoWeaponReward("weapon_glock", 0, 0), PROB_LOW),
      (new ChatSpyReward(basePlugin), PROB_LOW * 1.5f),
      (new ColorReward(Color.FromArgb(0, 255, 0), true), PROB_LOW),
      // (new ColorReward(Color.FromArgb(255, 0, 0), true), PROB_LOW),
      (new CannotUseReward(basePlugin, WeaponType.GRENADE), PROB_LOW),
      (new CannotScope(basePlugin), PROB_LOW),
      (new CannotRightKnife(basePlugin), PROB_LOW),
      (new CannotUseReward(basePlugin, WeaponType.SNIPERS), PROB_LOW),
      (new CannotUseReward(basePlugin, WeaponType.HEAVY), PROB_LOW),
      (new TransparentReward(), PROB_LOW / 2),
      (new AmmoWeaponReward("weapon_glock", 2, 0), PROB_LOW / 2),
      (new AmmoWeaponReward("weapon_negev", 0, 6), PROB_LOW / 2),
      (new CreditReward(50, locale), PROB_LOW), (new HPReward(1), PROB_LOW / 2),

      // Very low
      (new FakeBombReward(), PROB_VERY_LOW * 2),
      (new CannotLeftKnife(basePlugin), PROB_VERY_LOW),
      (new NoWeaponReward(), PROB_VERY_LOW),
      (new CannotUseReward(basePlugin, WeaponType.SMGS), PROB_VERY_LOW),
      (new CannotUseReward(basePlugin, WeaponType.PISTOLS), PROB_VERY_LOW),
      (new CannotUseReward(basePlugin, WeaponType.RIFLES), PROB_VERY_LOW),
      (new RandomTeleportReward(provider.GetService<IZoneManager>()),
        PROB_VERY_LOW),
      (new BombReward(bomb), PROB_VERY_LOW),
      (new CreditReward(-100, locale), PROB_VERY_LOW),
      (new CreditReward(500, locale), PROB_VERY_LOW),
      (new AmmoWeaponReward("weapon_deagle", 1, 0), PROB_VERY_LOW / 2),
      (new AmmoWeaponReward("weapon_awp", 1, 0), PROB_VERY_LOW / 4),

      // Extremely low
      (new CannotUseReward(basePlugin, WeaponType.KNIVES), PROB_EXTREMELY_LOW),
      (new CannotUseReward(basePlugin, WeaponType.GUNS), PROB_EXTREMELY_LOW),
      (new AmmoWeaponReward("weapon_awp", 3, 0), PROB_EXTREMELY_LOW),
      (new WeaponReward("weapon_glock"), PROB_EXTREMELY_LOW),
      (new CreditReward(1000, locale), PROB_EXTREMELY_LOW),
      (new CreditReward(-5000, locale), PROB_EXTREMELY_LOW / 2),

      // Lottery
      (new CreditReward(10000, locale), PROB_LOTTERY)
    ]);
  }

  public IRTDReward GenerateReward(int? id) {
    var effectiveTotal = id == null ?
      totalWeight :
      rewards.Where(reward
          => reward.Item1.Enabled && reward.Item1.CanGrantReward(id.Value))
       .Select(reward => reward.Item2)
       .Sum();
    var roll = rng.NextDouble() * effectiveTotal;

    foreach (var reward in rewards.Where(reward => reward.Item1.Enabled)) {
      if (id != null && !reward.Item1.CanGrantReward(id.Value)) continue;
      roll -= reward.Item2;
      if (roll <= 0) return reward.Item1;
    }

    throw new InvalidOperationException("No reward was generated. (" + roll
      + ")");
  }

  public IEnumerator<(IRTDReward, float)> GetEnumerator() {
    return rewards.Where(r => r.Item1.Enabled).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
  public int Count => rewards.Count(r => r.Item1.Enabled);
}