using CounterStrikeSharp.API.Core.Capabilities;

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
}