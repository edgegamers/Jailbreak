using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Logs.Listeners;
using Jailbreak.Logs.Tags;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs;

public static class LogsServiceExtension {
  public static void AddJailbreakLogs(this IServiceCollection services) {
    services.AddPluginBehavior<IRichLogService, LogsManager>();
    services.AddTransient<ILogService>(provider
      => provider.GetRequiredService<IRichLogService>());

    services.AddPluginBehavior<LogsCommand>();

    services.AddPluginBehavior<LogEntityListeners>();
    services.AddPluginBehavior<LogDamageListeners>();

    //	PlayerTagHelper is a lower-level class that avoids dependency loops.
    services.AddTransient<IRichPlayerTag, PlayerTagHelper>();
    services.AddTransient<IPlayerTag, PlayerTagHelper>();
  }
}