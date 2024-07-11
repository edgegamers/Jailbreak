using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.SpecialTreatment;

public class SpecialTreatmentBehavior(IPlayerStateFactory factory,
  IRebelService rebel, ISpecialTreatmentNotifications notifications)
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
    setSpecialColor(player, /* hasSt */ true);

    notifications.Granted.ToPlayerChat(player).ToPlayerCenter(player);

    notifications.GrantedTo(player).ToAllChat();
  }

  public void Revoke(CCSPlayerController player) {
    //  Player is already revoked
    if (!IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = false;

    setSpecialColor(player, false);

    notifications.Revoked.ToPlayerChat(player).ToPlayerCenter(player);

    notifications.RevokedFrom(player).ToAllChat();
  }

  private void setSpecialColor(CCSPlayerController player, bool hasSt) {
    if (!player.IsValid || player.Pawn.Value == null) return;

    var color = hasSt ?
      Color.FromArgb(254, 0, 255, 0) :
      Color.FromArgb(254, 255, 255, 255);

    player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
    player.Pawn.Value.Render     = color;
    Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity",
      "m_clrRender");
  }

  private class SpecialTreatmentState {
    public bool HasSpecialTreatment { get; set; }
  }
}