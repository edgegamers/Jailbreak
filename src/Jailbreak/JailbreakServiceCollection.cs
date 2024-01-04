using CounterStrikeSharp.API.Core;

using Jailbreak.Config;
using Jailbreak.Generic;
using Jailbreak.Public.Configuration;
using Jailbreak.Teams;
using Jailbreak.Warden;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak;

/// <summary>
///     Class that auto-registers all jailbreak services and classes.
/// </summary>
public class JailbreakServiceCollection : IPluginServiceCollection<Jailbreak>
{
	/// <inheritdoc />
	public void ConfigureServices(IServiceCollection serviceCollection)
	{
		//	Do we want to make this scoped?
		//	Not sure how this will behave with multiple rounds and whatnot.
		serviceCollection.AddTransient<IConfigService, ConfigService>();

		serviceCollection.AddJailbreakGeneric();
		serviceCollection.AddJailbreakWarden();
		serviceCollection.AddJailbreakTeams();
	}
}