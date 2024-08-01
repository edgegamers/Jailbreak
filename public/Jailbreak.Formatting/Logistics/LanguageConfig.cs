using Jailbreak.Formatting.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Formatting.Logistics;

public class LanguageConfig<TDialect> where TDialect : IDialect {
  private readonly IServiceCollection collection;

  public LanguageConfig(IServiceCollection collection) {
    this.collection = collection;
  }

  public void WithGenericCommand<TGenericCommand>()
    where TGenericCommand : class, ILanguage<TDialect>,
    IGenericCommandNotifications {
    collection.AddSingleton<IGenericCommandNotifications, TGenericCommand>();
  }

  public void WithRatio<TRatio>()
    where TRatio : class, ILanguage<TDialect>, IRatioNotifications {
    collection.AddSingleton<IRatioNotifications, TRatio>();
  }

  public void WithWarden<TWarden>()
    where TWarden : class, ILanguage<TDialect>, IWardenNotifications {
    collection.AddSingleton<IWardenNotifications, TWarden>();
  }

  public void WithRebel<TRebel>()
    where TRebel : class, ILanguage<TDialect>, IRebelNotifications {
    collection.AddSingleton<IRebelNotifications, TRebel>();
  }

  public void WithJihadC4<TJihadC4>()
    where TJihadC4 : class, ILanguage<TDialect>, IJihadC4Notifications {
    collection.AddSingleton<IJihadC4Notifications, TJihadC4>();
  }

  public void WithSpecialDay<TSpecialDay>()
    where TSpecialDay : class, ILanguage<TDialect>, ISpecialDayMessages {
    collection.AddSingleton<ISpecialDayMessages, TSpecialDay>();
  }

  public void WithLogging<TLogging>()
    where TLogging : class, ILanguage<TDialect>, ILogMessages {
    collection.AddSingleton<ILogMessages, TLogging>();
  }

  public void WithRollCommand<TRollCommand>()
    where TRollCommand : class, ILanguage<TDialect>, IRollCommandNotications {
    collection.AddSingleton<IRollCommandNotications, TRollCommand>();
  }

  public void WithLastRequest<TLastRequest>()
    where TLastRequest : class, ILanguage<TDialect>, ILastRequestMessages {
    collection.AddSingleton<ILastRequestMessages, TLastRequest>();
  }

  public void WithSpecialTreatment<TSpecialTreatment>()
    where TSpecialTreatment : class, ILanguage<TDialect>,
    ISpecialTreatmentNotifications {
    collection
     .AddSingleton<ISpecialTreatmentNotifications, TSpecialTreatment>();
  }

  public void WithMute<TMute>()
    where TMute : class, ILanguage<TDialect>, IPeaceMessages {
    collection.AddSingleton<IPeaceMessages, TMute>();
  }

  public void WithRaceLR<TRaceLR>()
    where TRaceLR : class, ILanguage<TDialect>, IRaceLRMessages {
    collection.AddSingleton<IRaceLRMessages, TRaceLR>();
  }

  public void WithLastGuard<TLastGuard>()
    where TLastGuard : class, ILanguage<TDialect>, ILastGuardNotifications {
    collection.AddSingleton<ILastGuardNotifications, TLastGuard>();
  }

  public void WithOpenCommand<TOpenCommand>()
    where TOpenCommand : class, ILanguage<TDialect>, IOpenCommandMessages {
    collection.AddSingleton<IOpenCommandMessages, TOpenCommand>();
  }
}