using System.Reflection;

using CounterStrikeSharp.API.Core;

using Jailbreak.Config;
using Jailbreak.English.Rebel;
using Jailbreak.English.Teams;
using Jailbreak.English.Warden;
using Jailbreak.Formatting.Languages;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Generic;
using Jailbreak.Logs;
using Jailbreak.Public.Configuration;
using Jailbreak.Rebel;
using Jailbreak.Teams;
using Jailbreak.Warden;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jailbreak;

/// <summary>
/// Class that auto-registers all jailbreak services and classes.
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
		serviceCollection.AddJailbreakRebel();
		serviceCollection.AddLogsService();

		//	Add in english localization
		serviceCollection.AddLanguage<Formatting.Languages.English>(config =>
		{
			config.WithRatio<RatioNotifications>();
			config.WithWarden<WardenNotifications>();
			config.WithRebel<RebelNotifications>();
		});
	}
}
