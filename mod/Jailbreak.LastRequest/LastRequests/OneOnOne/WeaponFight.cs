using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest.LastRequests.OneOnOne;

public class WeaponFight : AbstractLastRequest
{
    public WeaponFight(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin, prisoner, guard)
    {
    }
    protected override void OnStart()
    {
        throw new NotImplementedException();
    }
}