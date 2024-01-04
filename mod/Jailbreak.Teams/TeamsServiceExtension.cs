using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Teams;
using Jailbreak.Teams.Queue;
using Jailbreak.Teams.Ratio;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Teams;

public static class TeamsServiceExtension
{
	public static void AddJailbreakTeams(this IServiceCollection collection)
	{
		collection.AddConfig<RatioConfig>("ratio");

		collection.AddPluginBehavior<IGuardQueue, QueueBehavior>();
		collection.AddPluginBehavior<RatioBehavior>();
	}
}
