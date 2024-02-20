using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ILastRequestMessages
{
    public IView LastRequestEnabled();
    public IView LastRequestDisabled();
    public IView InvalidLastRequest(string query);
    public IView InvalidPlayerChoice(CCSPlayerController player, string reason);
}