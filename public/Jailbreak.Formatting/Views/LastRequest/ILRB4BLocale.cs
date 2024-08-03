using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.LastRequest;

/// <summary>
///   Last Request Bullet 4 Bullet Locale
/// </summary>
public interface ILRB4BLocale : ILRLocale {
  public IView PlayerGoesFirst(CCSPlayerController player);
  public IView WeaponSelected(CCSPlayerController player, string weapon);
}