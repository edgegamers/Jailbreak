﻿using CounterStrikeSharp.API.Core.Capabilities;
using GangsAPI;
using MAULActainShared.plugin;
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

  public static PluginCapability<IGangPlugin> GangsCapability { get; } =
    new("gangs:core");

  public static IMStat? Stats {
    get {
      try { return StatsCapability.Get(); } catch (KeyNotFoundException) {
        return null;
      }
    }
  }

  public static IActain? Actain {
    get {
      try { return ActainCapability.Get(); } catch (KeyNotFoundException) {
        return null;
      }
    }
  }

  public static IGangPlugin? Gangs {
    get {
      try { return GangsCapability.Get(); } catch (KeyNotFoundException) {
        return null;
      }
    }
  }
}