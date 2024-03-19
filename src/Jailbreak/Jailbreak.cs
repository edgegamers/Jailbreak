using System.Collections.Immutable;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jailbreak;

/// <summary>
///     The classic Jail gamemode, ported to Counter-Strike 2.
/// </summary>
public class Jailbreak : BasePlugin
{
    private IReadOnlyList<IPluginBehavior>? _extensions;

    private readonly IServiceProvider _provider;
    private IServiceScope? _scope;

    /// <summary>
    /// The Jailbreak plugin.
    /// </summary>
    /// <param name="provider"></param>
    public Jailbreak(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string ModuleName => "Jailbreak";

    /// <inheritdoc />
    public override string ModuleVersion => $"{GitVersionInformation.SemVer} ({GitVersionInformation.ShortSha})";

    /// <inheritdoc />
    public override string ModuleAuthor => "EdgeGamers Development";

    /// <inheritdoc />
    public override void Load(bool hotReload)
    {

        RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
        {
            manifest.AddResource("particles/lastguard_beacon.vpcf");
            manifest.AddResource("particles/explosion_c4_500_fallback.vpcf");
        });

        Logger.LogInformation("[Jailbreak] Loading...");

        _scope = _provider.CreateScope();
        _extensions = _scope.ServiceProvider.GetServices<IPluginBehavior>()
            .ToImmutableList();

        Logger.LogInformation("[Jailbreak] Found {@BehaviorCount} behaviors.", _extensions.Count);

        foreach (var extension in _extensions)
        {
            //	Register all event handlers on the extension object
            RegisterAllAttributes(extension);
            
            //	Tell the extension to start it's magic
            extension.Start(this);

            Logger.LogInformation("[Jailbreak] Loaded behavior {@Behavior}", extension.GetType().FullName);
        }

        base.Load(hotReload);
    }

    /// <inheritdoc />
    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("[Jailbreak] Shutting down...");

        if (_extensions != null)
            foreach (var extension in _extensions)
                extension.Dispose();

        //	Dispose of original extensions scope
        //	When loading again we will get a new scope to avoid leaking state.
        _scope?.Dispose();

        base.Unload(hotReload);
    }
}
