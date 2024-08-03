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
    serviceCollection.AddSingleton<ILogLocale, LogLocale>();
    serviceCollection.AddSingleton<IRebelLocale, RebelLocale>();
    serviceCollection.AddSingleton<IGenericCmdLocale, GenericCmdLocale>();
    serviceCollection.AddSingleton<ILRLocale, LastRequestLocale>();
    serviceCollection.AddSingleton<ILRCFLocale, CoinflipLocale>();
    serviceCollection.AddSingleton<ILRRPSLocale, RPSLocale>();
    serviceCollection.AddSingleton<ILRRaceLocale, RaceLocale>();
    serviceCollection.AddSingleton<ILGLocale, IlgLocale>();
    serviceCollection.AddSingleton<ISDLocale, IsdLocale>();
    serviceCollection.AddSingleton<IWardenLocale, WardenLocale>();
    serviceCollection.AddSingleton<IWardenSTLocale, IstLocale>();
    serviceCollection.AddSingleton<IWardenCmdOpenLocale, WardenCmdOpenLocale>();
    serviceCollection.AddSingleton<IWardenCmdRollLocale, WardenCmdRollLocale>();
    serviceCollection.AddSingleton<IWardenPeaceLocale, WardenPeaceLocale>();
    serviceCollection.AddSingleton<IC4Locale, C4Locale>();

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
  }
}