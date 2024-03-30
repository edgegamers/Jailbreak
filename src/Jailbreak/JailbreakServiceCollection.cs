using System.Reflection;

using CounterStrikeSharp.API.Core;
using Jailbreak.Config;
using Jailbreak.Debug;
using Jailbreak.English.Generic;
using Jailbreak.English.LastRequest;
using Jailbreak.English.Logs;
using Jailbreak.English.Mute;
using Jailbreak.English.Rebel;
using Jailbreak.English.Warden;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Generic;
using Jailbreak.LastRequest;
using Jailbreak.Logs;
using Jailbreak.Mute;
using Jailbreak.Public.Configuration;
using Jailbreak.Rebel;
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
        serviceCollection.AddJailbreakRebel();
        serviceCollection.AddJailbreakMute();
        serviceCollection.AddJailbreakWarden();
        serviceCollection.AddJailbreakDebug();
        serviceCollection.AddJailbreakLastRequest();

		//	Add in english localization
		serviceCollection.AddLanguage<Formatting.Languages.English>(config =>
		{
			config.WithGenericCommand<GenericCommandNotifications>();
			config.WithWarden<WardenNotifications>();
			config.WithRebel<RebelNotifications>();
			config.WithLogging<LogMessages>();
			config.WithLastRequest<LastRequestMessages>();
			config.WithSpecialTreatment<SpecialTreatmentNotifications>();
			config.WithMute<PeaceMessages>();
		});
	}
}
