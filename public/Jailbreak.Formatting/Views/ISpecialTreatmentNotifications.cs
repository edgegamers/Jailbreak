using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialTreatmentNotifications {
  public IView Granted { get; }

  public IView Revoked { get; }

  public IView GrantedTo(CCSPlayerController player);

  public IView RevokedFrom(CCSPlayerController player);
}