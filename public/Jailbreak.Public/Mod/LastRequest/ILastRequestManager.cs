using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestManager : IPluginBehavior
{
    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; }
    
    void InitiateLastRequest(AbstractLastRequest lastRequest);
    
}