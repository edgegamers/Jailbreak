using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public static class LastRequestExtension {
  public static void AddJailbreakLastRequest(
    this IServiceCollection collection) {
    collection.AddPluginBehavior<ILastRequestFactory, LastRequestFactory>();
    collection.AddPluginBehavior<ILastRequestManager, LastRequestManager>();
    collection.AddPluginBehavior<LastRequestCommand>();
    collection.AddPluginBehavior<EndRaceCommand>();
    collection.AddPluginBehavior<ILastRequestRebelManager, LastRequestRebelManager>();
    collection.AddPluginBehavior<LastRequestRebelCommand>();
  }
}