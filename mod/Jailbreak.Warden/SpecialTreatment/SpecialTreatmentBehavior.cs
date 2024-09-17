using System.Drawing;
using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.SpecialTreatment;

public class SpecialTreatmentBehavior(IPlayerStateFactory factory,
  IRebelService rebel, IWardenSTLocale notifications)
  : IPluginBehavior, ISpecialTreatmentService {
  private readonly IPlayerState<SpecialTreatmentState> sts =
    factory.Round<SpecialTreatmentState>();

  public bool IsSpecialTreatment(CCSPlayerController player) {
    return sts.Get(player).HasSpecialTreatment;
  }

  public void Grant(CCSPlayerController player) {
    //  Player is already granted ST
    if (IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = true;

    if (rebel.IsRebel(player)) rebel.UnmarkRebel(player);
    setSpecialColor(player, true);
    player.ColorScreen(Color.FromArgb(16, 0, 255, 0), 999999, 0.2f,
      PlayerExtensions.FadeFlags.FADE_OUT);

    notifications.Granted.ToChat(player).ToCenter(player);

    notifications.GrantedTo(player).ToAllChat();
  }

  public void Revoke(CCSPlayerController player) {
    //  Player is already revoked
    if (!IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = false;

    setSpecialColor(player, false);
    player.ColorScreen(Color.FromArgb(16, 0, 255, 0), 0f, 1.5f);

    notifications.Revoked.ToChat(player).ToCenter(player);

    notifications.RevokedFrom(player).ToAllChat();
  }

  private void setSpecialColor(CCSPlayerController player, bool hasSt) {
    if (!player.IsValid || player.Pawn.Value == null) return;

    var color = hasSt ?
      Color.FromArgb(255, 0, 255, 0) :
      Color.FromArgb(255, 255, 255, 255);

    player.SetColor(color);
  }

  private class SpecialTreatmentState {
    public bool HasSpecialTreatment { get; set; }
  }
}