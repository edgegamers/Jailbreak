using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Formatting.Logistics;

public class LanguageConfig<TDialect> where TDialect : IDialect {
  private readonly IServiceCollection collection;

  public LanguageConfig(IServiceCollection collection) {
    this.collection = collection;
  }

  // public void WithGenericCommand<TGenericCommand>()
  //   where TGenericCommand : class, ILanguage<TDialect>,
  //   IGenericCmdLocale {
  //   collection.AddSingleton<IGenericCmdLocale, TGenericCommand>();
  // }
  //
  // public void WithWarden<TWarden>()
  //   where TWarden : class, ILanguage<TDialect>, IWardenLocale {
  //   collection.AddSingleton<IWardenLocale, TWarden>();
  // }
  //
  // public void WithRebel<TRebel>()
  //   where TRebel : class, ILanguage<TDialect>, IRebelLocale {
  //   collection.AddSingleton<IRebelLocale, TRebel>();
  // }
  //
  // public void WithJihadC4<TJihadC4>()
  //   where TJihadC4 : class, ILanguage<TDialect>, IC4Locale {
  //   collection.AddSingleton<IC4Locale, TJihadC4>();
  // }
  //
  // public void WithSpecialDay<TSpecialDay>()
  //   where TSpecialDay : class, ILanguage<TDialect>, ISDLocale {
  //   collection.AddSingleton<ISDLocale, TSpecialDay>();
  // }
  //
  // public void WithLogging<TLogging>()
  //   where TLogging : class, ILanguage<TDialect>, ILogLocale {
  //   collection.AddSingleton<ILogLocale, TLogging>();
  // }
  //
  // public void WithRollCommand<TRollCommand>()
  //   where TRollCommand : class, ILanguage<TDialect>, IWardenCmdRollLocale {
  //   collection.AddSingleton<IWardenCmdRollLocale, TRollCommand>();
  // }
  //
  // public void WithLastRequest<TLastRequest>()
  //   where TLastRequest : class, ILanguage<TDialect>, ILRLocale {
  //   collection.AddSingleton<ILRLocale, TLastRequest>();
  // }
  //
  // public void WithSpecialTreatment<TSpecialTreatment>()
  //   where TSpecialTreatment : class, ILanguage<TDialect>,
  //   IWardenSTLocale {
  //   collection
  //    .AddSingleton<IWardenSTLocale, TSpecialTreatment>();
  // }
  //
  // public void WithMute<TMute>()
  //   where TMute : class, ILanguage<TDialect>, IWardenPeaceLocale {
  //   collection.AddSingleton<IWardenPeaceLocale, TMute>();
  // }
  //
  // public void WithRaceLR<TRaceLR>()
  //   where TRaceLR : class, ILanguage<TDialect>, ILRRaceLocale {
  //   collection.AddSingleton<ILRRaceLocale, TRaceLR>();
  // }
  //
  // public void WithLastGuard<TLastGuard>()
  //   where TLastGuard : class, ILanguage<TDialect>, ILGLocale {
  //   collection.AddSingleton<ILGLocale, TLastGuard>();
  // }
  //
  // public void WithOpenCommand<TOpenCommand>()
  //   where TOpenCommand : class, ILanguage<TDialect>, IWardenCmdOpenLocale {
  //   collection.AddSingleton<IWardenCmdOpenLocale, TOpenCommand>();
  // }

  public void Configure(Dictionary<Type, Type> serviceMap) {
    foreach (var (service, implementation) in serviceMap) {
      var method = typeof(IServiceCollection).GetMethod("AddSingleton")
      ?.MakeGenericMethod(service, implementation);
      method?.Invoke(collection, null);
    }
  }
}