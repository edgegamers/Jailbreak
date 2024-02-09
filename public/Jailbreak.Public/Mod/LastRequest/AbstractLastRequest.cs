using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest : IPluginBehavior
{
    public string Name { get; }
    public LastRequestState State { get; protected set; }
    public LastRequestCategory Category { get; }
    
    protected Dictionary<Type, LastRequestOptions> options = new ();
    
    protected ISet<CCSPlayerController> members;
    protected BasePlugin plugin;

    protected AbstractLastRequest(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard)
    {
        this.plugin = plugin;
        members = new HashSet<CCSPlayerController> {prisoner, guard};
        // options[GenericLastRequestOptions] = new GenericLastRequestOptions(this);
    }

    public void Start(long delay = 0)
    {
        plugin.AddTimer(delay, (() =>
        {
           OnStart(); 
        }));
    }
    
    protected abstract void OnStart();
    
    public ISet<CCSPlayerController> GetCTs()
    {
        return members.Where(x => x.GetTeam() == CsTeam.CounterTerrorist).ToHashSet();
    }

    public ISet<CCSPlayerController> GetTs()
    {
        return members.Where(x => x.GetTeam() == CsTeam.Terrorist).ToHashSet();
    }

    public CCSPlayerController GetT()
    {
        return GetTs().First();
    }
}