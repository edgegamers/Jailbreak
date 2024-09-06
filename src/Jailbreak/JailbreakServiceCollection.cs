using CounterStrikeSharp.API.Core;
using Jailbreak.Debug;
using Jailbreak.English.Generic;
using Jailbreak.English.LastGuard;
using Jailbreak.English.LastRequest;
using Jailbreak.English.Logs;
using Jailbreak.English.Mute;
using Jailbreak.English.Rebel;
using Jailbreak.English.RTD;
using Jailbreak.English.SpecialDay;
using Jailbreak.English.Warden;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Formatting.Views.RTD;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Generic;
using Jailbreak.LastGuard;
using Jailbreak.LastRequest;
using Jailbreak.Logs;
using Jailbreak.Mute;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Rebel;
using Jailbreak.RTD;
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
    serviceCollection.AddSingleton<IC4Locale, C4Locale>();
    serviceCollection.AddSingleton<IGenericCmdLocale, GenericCmdLocale>();
    serviceCollection.AddSingleton<ILGLocale, LGLocale>();
    serviceCollection.AddSingleton<ILRB4BLocale, B4BLocale>();
    serviceCollection.AddSingleton<ILRCFLocale, CoinflipLocale>();
    serviceCollection.AddSingleton<ILRLocale, LastRequestLocale>();
    serviceCollection.AddSingleton<ILRRPSLocale, RPSLocale>();
    serviceCollection.AddSingleton<ILRRaceLocale, RaceLocale>();
    serviceCollection.AddSingleton<ILogLocale, LogLocale>();
    serviceCollection.AddSingleton<IRebelLocale, RebelLocale>();
    serviceCollection.AddSingleton<ISDLocale, SDLocale>();
    serviceCollection.AddSingleton<IWardenCmdOpenLocale, WardenCmdOpenLocale>();
    serviceCollection.AddSingleton<IWardenCmdRollLocale, WardenCmdRollLocale>();
    serviceCollection.AddSingleton<IWardenLocale, WardenLocale>();
    serviceCollection.AddSingleton<IWardenPeaceLocale, WardenPeaceLocale>();
    serviceCollection.AddSingleton<IWardenSTLocale, WardenSTLocale>();
    serviceCollection.AddSingleton<IRTDLocale, RTDLocale>();
    serviceCollection.AddSingleton<IAutoRTDLocale, AutoRTDLocale>();
    serviceCollection
     .AddSingleton<IWardenCmdChickenLocale, WardenCmdChickenLocale>();
    serviceCollection
     .AddSingleton<IWardenCmdSoccerLocale, WardenCmdSoccerLocale>();

    //	Do we want to make this scoped?
    //	Not sure how this will behave with multiple rounds and whatnot.
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
    serviceCollection.AddDiceRoll();
  }
}