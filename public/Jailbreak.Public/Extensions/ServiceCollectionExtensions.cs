using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	///     Add a <see cref="IPluginBehavior" /> to the global service collection
	/// </summary>
	/// <param name="collection"></param>
	/// <typeparam name="TExtension"></typeparam>
	public static void AddPluginBehavior<TExtension>(this IServiceCollection collection)
		where TExtension : class, IPluginBehavior
	{
		//	Add the root extension itself as a scoped service.
		//	This means every time Load is called in the main Jailbreak loader,
		//	the extension will be fetched and kept as a singleton for the duration
		//	until "Unload" is called.
		collection.AddScoped<TExtension>();

		collection.AddTransient<IPluginBehavior, TExtension>(provider => provider.GetRequiredService<TExtension>());
	}

	/// <summary>
	///     Add a <see cref="IPluginBehavior" /> to the global service collection
	/// </summary>
	/// <param name="collection"></param>
	/// <typeparam name="TExtension"></typeparam>
	/// <typeparam name="TInterface"></typeparam>
	public static void AddPluginBehavior<TInterface, TExtension>(this IServiceCollection collection)
		where TExtension : class, IPluginBehavior, TInterface
		where TInterface : class
	{
		//	Add the root extension itself as a scoped service.
		//	This means every time Load is called in the main Jailbreak loader,
		//	the extension will be fetched and kept as a singleton for the duration
		//	until "Unload" is called.
		collection.AddScoped<TExtension>();

		collection.AddTransient<TInterface, TExtension>(provider => provider.GetRequiredService<TExtension>());
		collection.AddTransient<IPluginBehavior, TExtension>(provider => provider.GetRequiredService<TExtension>());
	}

	/// <summary>
	///     Add an object to be loaded from the configuration file
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="sectionName">The section where the configuration object will be loaded from</param>
	/// <typeparam name="TConfig">The configuration object. Must auto-fill all default values!</typeparam>
	public static void AddConfig<TConfig>(this IServiceCollection collection, string sectionName)
		where TConfig : class, new()
	{
		//	Get the object by resolving IConfigService
		//	and use the Get<T>() method.

		//	Not *really* important... but do we want to fail here or return default if section
		//	isn't available?
		collection.AddTransient<TConfig>(provider => provider.GetRequiredService<IConfigService>()
			.Get<TConfig>(sectionName));
	}
}