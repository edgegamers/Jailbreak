using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Behaviors;

/// <summary>
/// An IPluginBehavior is a class that is registered just like a BasePlugin
/// in the Jailbreak plugin loader.
///
/// In order to register a IPluginBehavior, you need to call
/// <see cref="Extensions.ServiceCollectionExtensions.AddPluginBehavior"/>
///
/// IPluginBehaviors are public to any other plugin that has a definition
/// for the behavior class. For this reason, you should implement a behavior interface,
/// such as the built-in <see cref="Public.Mod.Mute.IMuteService"/>.
/// </summary>
public interface IPluginBehavior : IDisposable
{
	/// <summary>
	/// The plugin will be shut down soon.
	/// Remove anything that will outlast the plugin.
	/// Counter-Strike Sharp will release most resources
	/// for us (such as event hooks)
	/// </summary>
    void IDisposable.Dispose()
    {
    }

    /// <summary>
    /// The plugin will start imminently.
    /// Register all events & handlers here
    /// </summary>
    void Start(BasePlugin parent)
    {
    }
}
