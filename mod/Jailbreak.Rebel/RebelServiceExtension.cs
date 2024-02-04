using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Teams;
using Jailbreak.Teams;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Rebel;

public static class RebelServiceExtension
{
	public static void AddJailbreakRebel(this IServiceCollection collection)
	{
		collection.AddPluginBehavior<IRebelService, RebelManager>();	
		collection.AddPluginBehavior<RebelListener>();
	}
}
