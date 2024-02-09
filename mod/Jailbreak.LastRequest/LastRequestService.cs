using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestService : ILastRequestService
{
    public bool IsLastRequestActive()
    {
        throw new NotImplementedException();
    }

    public bool SetLastRequestActive(bool lastRequest)
    {
        throw new NotImplementedException();
    }

    public AbstractLastRequest? GetLastRequest(CCSPlayerController player)
    {
        throw new NotImplementedException();
    }

    public bool SetLastRequest(CCSPlayerController player, AbstractLastRequest lastRequest)
    {
        throw new NotImplementedException();
    }
}