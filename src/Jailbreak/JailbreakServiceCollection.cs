using System.Reflection;

using CounterStrikeSharp.API.Core;
using Jailbreak.Config;
using Jailbreak.Debug;
using Jailbreak.English.Generic;
using Jailbreak.English.Logs;
using Jailbreak.English.Rebel;
using Jailbreak.English.Teams;
using Jailbreak.English.Warden;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Generic;
using Jailbreak.Logs;
using Jailbreak.Public.Configuration;
using Jailbreak.Rebel;
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
        serviceCollection.AddJailbreakLogs();
        serviceCollection.AddJailbreakWarden();
        serviceCollection.AddJailbreakTeams();
        serviceCollection.AddJailbreakRebel();
        serviceCollection.AddJailbreakDebug();

		//	Add in english localization
		serviceCollection.AddLanguage<Formatting.Languages.English>(config =>
		{
			config.WithGenericCommand<GenericCommandNotifications>();
			config.WithRatio<RatioNotifications>();
			config.WithWarden<WardenNotifications>();
			config.WithRebel<RebelNotifications>();
            config.WithJihadC4<JihadC4Notifications>();
			config.WithLogging<LogMessages>();
		});
	}
}
