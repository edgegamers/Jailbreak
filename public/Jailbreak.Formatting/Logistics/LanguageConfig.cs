using Jailbreak.Formatting.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Formatting.Logistics;

public class LanguageConfig<TDialect>
	where TDialect : IDialect
{

	private IServiceCollection _collection;

	public LanguageConfig(IServiceCollection collection)
	{
		_collection = collection;
	}

	public void WithGenericCommand<TGenericCommand>()
		where TGenericCommand : class, ILanguage<TDialect>, IGenericCommandNotifications
		=> _collection.AddSingleton<IGenericCommandNotifications, TGenericCommand>();

	public void WithRatio<TRatio>()
		where TRatio : class, ILanguage<TDialect>, IRatioNotifications
		=> _collection.AddSingleton<IRatioNotifications, TRatio>();

	public void WithWarden<TWarden>()
		where TWarden : class, ILanguage<TDialect>, IWardenNotifications
		=> _collection.AddSingleton<IWardenNotifications, TWarden>();

    public void WithPeaceWarden<TPeaceWarden>()
		where TPeaceWarden : class, ILanguage<TDialect>, IWardenPeaceNotifications
		=> _collection.AddSingleton<IWardenPeaceNotifications, TPeaceWarden>();

    public void WithLastGuardWarden<TLastGuardWarden>()
    where TLastGuardWarden : class, ILanguage<TDialect>, IWardenLastGuardNotifications
    => _collection.AddSingleton<IWardenLastGuardNotifications, TLastGuardWarden>();

    public void WithRebel<TRebel>()
		where TRebel : class, ILanguage<TDialect>, IRebelNotifications
		=> _collection.AddSingleton<IRebelNotifications, TRebel>();

	public void WithLogging<TLogging>()
		where TLogging : class, ILanguage<TDialect>, ILogMessages
		=> _collection.AddSingleton<ILogMessages, TLogging>();

}
