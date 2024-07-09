using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialTreatmentNotifications {
  public IView GRANTED { get; }

  public IView REVOKED { get; }

  public IView GRANTED_TO(CCSPlayerController player);

  public IView REVOKED_FROM(CCSPlayerController player);
}