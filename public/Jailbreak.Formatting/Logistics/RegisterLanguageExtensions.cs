using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Formatting.Logistics;

public static class RegisterLanguageExtensions
{
    public static void AddLanguage<TDialect>(
        this IServiceCollection collection,
        Action<LanguageConfig<TDialect>> factory)
        where TDialect : IDialect
    {
        var config = new LanguageConfig<TDialect>(collection);

        factory(config);
    }
}