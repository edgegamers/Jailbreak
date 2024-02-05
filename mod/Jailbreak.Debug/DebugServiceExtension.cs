using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug;

public static class DebugServiceExtension
{
   public static void AddDebugService(this IServiceCollection services)
   {
      services.AddPluginBehavior<DebugCommand>();
   } 
}