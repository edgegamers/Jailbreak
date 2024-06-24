using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ILastGuardMessages
{
    public IView LastGuardActivated(CCSPlayerController guard, int prisonerHp);

    /// <summary>
    /// Message to print when last guard is over.
    /// </summary>
    /// <param name="winner">If null, round time expired</param>
    /// <returns></returns>
    public IView LastGuardOver(CCSPlayerController? winner);
}