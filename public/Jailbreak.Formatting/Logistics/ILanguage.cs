namespace Jailbreak.Formatting.Logistics;

/// <summary>
///   Specifies that this class is written in a specific language
///   Eg, ILanguage&lt;English> or ILanguage&lt;French>
/// </summary>
/// <typeparam name="TDialect"></typeparam>
public interface ILanguage<TDialect> where TDialect : IDialect { }