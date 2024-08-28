using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.RTD;

public static class RTDServiceExtensions {
  public static void AddDiceRoll(this IServiceCollection collection) {
    collection.AddPluginBehavior<IRewardGenerator, RewardGenerator>();
    collection.AddPluginBehavior<IRTDRewarder, RTDRewarder>();
    collection.AddPluginBehavior<RTDCommand>();
    collection.AddPluginBehavior<AutoRTDListener>();
    collection.AddPluginBehavior<RTDStatsCommand>();
  }
}