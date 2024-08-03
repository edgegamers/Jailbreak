using CounterStrikeSharp.API.Core;
using Jailbreak.Config;
using Jailbreak.Debug;
using Jailbreak.English.Generic;
using Jailbreak.English.LastGuard;
using Jailbreak.English.LastRequest;
using Jailbreak.English.Logs;
using Jailbreak.English.Mute;
using Jailbreak.English.Rebel;
using Jailbreak.English.SpecialDay;
using Jailbreak.English.Warden;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Generic;
using Jailbreak.LastGuard;
using Jailbreak.LastRequest;
using Jailbreak.Logs;
using Jailbreak.Mute;
using Jailbreak.Public.Configuration;
using Jailbreak.Rebel;
using Jailbreak.SpecialDay;
using Jailbreak.Warden;
using Jailbreak.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak;

/// <summary>
///   Class that auto-registers all jailbreak services and classes.
/// </summary>
public class JailbreakServiceCollection : IPluginServiceCollection<Jailbreak> {
  /// <inheritdoc />
  public void ConfigureServices(IServiceCollection serviceCollection) {
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
    serviceCollection.AddJailbreakLastGuard();
    serviceCollection.AddJailbreakSpecialDay();
    serviceCollection.AddJailbreakZones();

    //	Add in english localization
    serviceCollection.AddLanguage<Formatting.Languages.English>(config => {
      var serviceMap = new Dictionary<Type, Type> {
        { typeof(IC4Locale), typeof(Ic4Locale) },
        { typeof(IGenericCmdLocale), typeof(GenericCmdLocale) },
        { typeof(ILGLocale), typeof(IlgLocale) },
        { typeof(ILRCFLocale), typeof(CoinflipLocale) },
        { typeof(ILRLocale), typeof(LastRequestLocale) },
        { typeof(ILRRPSLocale), typeof(RPSLocale) },
        { typeof(ILRRaceLocale), typeof(RaceLocale) },
        { typeof(ILogLocale), typeof(LogLocale) },
        { typeof(IRebelLocale), typeof(RebelLocale) },
        { typeof(ISDLocale), typeof(IsdLocale) },
        { typeof(IWardenSTLocale), typeof(IstLocale) }, {
          typeof(IWardenCmdOpenLocale),
          typeof(WardenCmdOpenCommandNotifications)
        }, {
          typeof(IWardenCmdRollLocale),
          typeof(WardenCmdRollCommandNotifications)
        },
        { typeof(IWardenLocale), typeof(WardenLocale) },
        { typeof(IWardenPeaceLocale), typeof(WardenPeaceLocale) }
      };
      config.Configure(serviceMap);
    });
  }
}