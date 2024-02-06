using Jailbreak.Formatting.Views;
using Jailbreak.Logs.Tags;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs;

public static class LogsServiceExtension
{
   public static void AddJailbreakLogs(this IServiceCollection services)
   {
	   services.AddPluginBehavior<ILogService, LogsManager>();

	   services.AddPluginBehavior<LogsCommand>();
	   services.AddPluginBehavior<LogsListeners>();

	   //	PlayerTagHelper is a lower-level class that avoids dependency loops.
	   services.AddTransient<IRichPlayerTag, PlayerTagHelper>();
	   services.AddTransient<IPlayerTag, PlayerTagHelper>();
   }
}
