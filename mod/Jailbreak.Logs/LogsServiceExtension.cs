﻿using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs;

public static class LogsServiceExtension
{
   public static void AddLogsService(this IServiceCollection services)
   {
      services.AddPluginBehavior<ILogService, LogsManager>();
      
      services.AddPluginBehavior<LogsCommand>();
   } 
}