using api.plugin;
using CounterStrikeSharp.API.Core.Capabilities;
using MStatsShared;

namespace Jailbreak.Public;

/// <summary>
///   The entry point to the Jailbreak API
/// </summary>
public static class API {
  /// <summary>
  ///   Grants access to the currently running service provider, if there is one.
  ///   The service provider can be used to get instantiated IPluginBehaviors and other
  ///   objects exposed to Jailbreak mods
  /// </summary>
  public static PluginCapability<IServiceProvider> Provider { get; } =
    new("jailbreak:core");

  public static PluginCapability<IActain> ActainCapability { get; } =
    new("maulactain:core");

  public static PluginCapability<IMStat> StatsCapability { get; } =
    new("mstats:core");

  public static IMStat? Stats {
    get {
      try { return StatsCapability.Get(); } catch (KeyNotFoundException _) {
        return null;
      }
    }
  }

  public static IActain? Actain {
    get {
      try { return ActainCapability.Get(); } catch (KeyNotFoundException _) {
        return null;
      }
    }
  }
}