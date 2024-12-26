using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Formatting.Views.LastRequest;

/// <summary>
///   Last Request Locale
/// </summary>
public interface ILRLocale {
  public IView DamageBlockedInsideLastRequest { get; }
  public IView DamageBlockedNotInSameLR { get; }
  public IView LastRequestEnabled();
  public IView LastRequestDisabled();
  public IView LastRequestNotEnabled();
  public IView InvalidLastRequest(string query);
  public IView InformLastRequest(AbstractLastRequest lr);

  [Obsolete("Unused, use InformLastRequest instead")]
  public IView AnnounceLastRequest(AbstractLastRequest lr);

  public IView LastRequestDecided(AbstractLastRequest lr, LRResult result);
  public IView CannotLR(string reason);
  public IView CannotLR(CCSPlayerController player, string reason);
  public IView LastRequestCountdown(int seconds);

  public IView WinByDefault(CCSPlayerController player);
  public IView WinByHealth(CCSPlayerController player);
  public IView WinByReason(CCSPlayerController player, string reason);
  public IView Win(CCSPlayerController player);

  public IView LastRequestRebel(CCSPlayerController player, int tHealth);
  public IView LastRequestRebelDisabled();
  public IView CannotLastRequestRebelCt();
}