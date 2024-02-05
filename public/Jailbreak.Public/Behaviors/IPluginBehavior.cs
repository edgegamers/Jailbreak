using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Behaviors;

/// <summary>
///     A plugin extension class that is
/// </summary>
public interface IPluginBehavior : IDisposable
{
    void IDisposable.Dispose()
    {
    }

    /// <summary>
    ///     Tells the plugin that it will be starting imminently
    /// </summary>
    void Start(BasePlugin parent)
    {
    }
}