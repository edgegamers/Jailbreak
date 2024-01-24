using CounterStrikeSharp.API.Core;

using Jailbreak.Generic.Coroutines;
using Jailbreak.Generic.PlayerState;
using Jailbreak.Generic.PlayerState.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Generic;

public static class GenericServiceExtension
{
	public static void AddJailbreakGeneric(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddPluginBehavior<AliveStateTracker>();
		serviceCollection.AddPluginBehavior<GlobalStateTracker>();
		serviceCollection.AddPluginBehavior<RoundStateTracker>();

		serviceCollection.AddTransient<IPlayerStateFactory, PlayerStateFactory>();

		serviceCollection.AddPluginBehavior<ICoroutines, CoroutineManager>();
	}
}
