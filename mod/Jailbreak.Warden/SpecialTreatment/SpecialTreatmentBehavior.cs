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

public class SpecialTreatmentBehavior : IPluginBehavior, ISpecialTreatmentService
{
    private IPlayerState<SpecialTreatmentState> _sts;
    private IRebelService _rebel;
    private ISpecialTreatmentNotifications _notifications;

    public SpecialTreatmentBehavior(IPlayerStateFactory factory, IRebelService rebel, ISpecialTreatmentNotifications notifications)
    {
        _sts = factory.Round<SpecialTreatmentState>();

        _rebel = rebel;
        _notifications = notifications;
    }

    private class SpecialTreatmentState
    {
        public bool HasSpecialTreatment { get; set; } = false;
    }

    public bool IsSpecialTreatment(CCSPlayerController player)
    {
        return _sts.Get(player)
            .HasSpecialTreatment;
    }

    public void Grant(CCSPlayerController player)
    {
        //  Player is already granted ST
        if (IsSpecialTreatment(player))
            return;

        _sts.Get(player).HasSpecialTreatment = true;

        _rebel.UnmarkRebel(player);
        this.SetPlayerColor(player, /* hasSt */ true);

        _notifications.GRANTED
            .ToPlayerChat(player)
            .ToPlayerCenter(player);

        _notifications.GRANTED_TO(player)
            .ToAllChat();
    }

    public void Revoke(CCSPlayerController player)
    {
        //  Player is already revoked
        if (!IsSpecialTreatment(player))
            return;

        _sts.Get(player).HasSpecialTreatment = false;

        this.SetPlayerColor(player, /* hasSt */ false);

        _notifications.REVOKED
            .ToPlayerChat(player)
            .ToPlayerCenter(player);

        _notifications.REVOKED_FROM(player)
            .ToAllChat();
    }

    private void SetPlayerColor(CCSPlayerController player, bool hasSt)
    {
        if (!player.IsValid || player.Pawn.Value == null)
            return;

        var color = hasSt
            ? Color.FromArgb(254, 0, 255, 0)
            : Color.FromArgb(254, 255, 255, 255);

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.Pawn.Value.Render = color;
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
    }
}
