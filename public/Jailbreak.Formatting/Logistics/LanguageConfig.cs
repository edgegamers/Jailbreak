using Jailbreak.Formatting.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Formatting.Logistics;

public class LanguageConfig<TDialect>
    where TDialect : IDialect
{
    private readonly IServiceCollection _collection;

    public LanguageConfig(IServiceCollection collection)
    {
        _collection = collection;
    }

    public void WithGenericCommand<TGenericCommand>()
        where TGenericCommand : class, ILanguage<TDialect>, IGenericCommandNotifications
    {
        _collection.AddSingleton<IGenericCommandNotifications, TGenericCommand>();
    }

    public void WithRatio<TRatio>()
        where TRatio : class, ILanguage<TDialect>, IRatioNotifications
    {
        _collection.AddSingleton<IRatioNotifications, TRatio>();
    }

    public void WithWarden<TWarden>()
        where TWarden : class, ILanguage<TDialect>, IWardenNotifications
    {
        _collection.AddSingleton<IWardenNotifications, TWarden>();
    }

    public void WithRebel<TRebel>()
        where TRebel : class, ILanguage<TDialect>, IRebelNotifications
    {
        _collection.AddSingleton<IRebelNotifications, TRebel>();
    }

    public void WithJihadC4<TJihadC4>()
        where TJihadC4 : class, ILanguage<TDialect>, IJihadC4Notifications
    {
        _collection.AddSingleton<IJihadC4Notifications, TJihadC4>();
    }

    public void WithSpecialDay<TSpecialDay>()
        where TSpecialDay : class, ILanguage<TDialect>, ISpecialDayNotifications
    {
        _collection.AddSingleton<ISpecialDayNotifications, TSpecialDay>();
    }

    public void WithLogging<TLogging>()
        where TLogging : class, ILanguage<TDialect>, ILogMessages
    {
        _collection.AddSingleton<ILogMessages, TLogging>();
    }

    public void WithRollCommand<TRollCommand>()
        where TRollCommand : class, ILanguage<TDialect>, IRollCommandNotications
    {
        _collection.AddSingleton<IRollCommandNotications, TRollCommand>();
    }

    public void WithLastRequest<TLastRequest>()
        where TLastRequest : class, ILanguage<TDialect>, ILastRequestMessages
    {
        _collection.AddSingleton<ILastRequestMessages, TLastRequest>();
    }

    public void WithSpecialTreatment<TSpecialTreatment>()
        where TSpecialTreatment : class, ILanguage<TDialect>, ISpecialTreatmentNotifications
    {
        _collection.AddSingleton<ISpecialTreatmentNotifications, TSpecialTreatment>();
    }

    public void WithMute<TMute>()
        where TMute : class, ILanguage<TDialect>, IPeaceMessages
    {
        _collection.AddSingleton<IPeaceMessages, TMute>();
    }

    public void WithRaceLR<TRaceLR>()
        where TRaceLR : class, ILanguage<TDialect>, IRaceLRMessages
    {
        _collection.AddSingleton<IRaceLRMessages, TRaceLR>();
    }

    public void WithLastGuard<TLastGuard>()
        where TLastGuard : class, ILanguage<TDialect>, ILastGuardNotifications
    {
        _collection.AddSingleton<ILastGuardNotifications, TLastGuard>();
    }
}