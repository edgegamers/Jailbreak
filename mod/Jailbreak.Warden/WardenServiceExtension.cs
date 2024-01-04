using CounterStrikeSharp.API.Core;

using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Warden.Commands;
using Jailbreak.Warden.Global;
using Jailbreak.Warden.Queue;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden;

public static class WardenServiceExtension
{
	public static void AddJailbreakWarden(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddPluginBehavior<IWardenService, WardenBehavior>();
		serviceCollection.AddPluginBehavior<IWardenSelectionService, WardenSelectionBehavior>();

		serviceCollection.AddPluginBehavior<WardenCommandsBehavior>();
	}
}
