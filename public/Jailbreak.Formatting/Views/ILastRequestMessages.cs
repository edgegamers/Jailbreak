using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Formatting.Views;

public interface ILastRequestMessages
{
    public IView LastRequestEnabled();
    public IView LastRequestDisabled();
    public IView InvalidLastRequest(string query);
    public IView InvalidPlayerChoice(CCSPlayerController player, string reason);
    public IView InformLastRequest(AbstractLastRequest lr);
    public IView AnnounceLastRequest(AbstractLastRequest lr);
    public IView LastRequestDecided(AbstractLastRequest lr, LRResult result);
}