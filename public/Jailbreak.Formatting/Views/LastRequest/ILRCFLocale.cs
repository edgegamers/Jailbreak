using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.LastRequest;

/// <summary>
///   Last Request Coin Flip Locale
/// </summary>
public interface ILRCFLocale : ILRLocale {
  public IView FailedToChooseInTime(bool choice);
  public IView GuardChose(CCSPlayerController guard, bool choice);
  public IView CoinLandsOn(bool heads);
}