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

public class SpecialTreatmentBehavior(
    IPlayerStateFactory factory,
    IRebelService rebel,
    ISpecialTreatmentNotifications notifications)
    : IPluginBehavior, ISpecialTreatmentService
{
    private readonly IPlayerState<SpecialTreatmentState> _sts = factory.Round<SpecialTreatmentState>();

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

        rebel.UnmarkRebel(player);
        this.SetSpecialColor(player, /* hasSt */ true);

        notifications.GRANTED
            .ToPlayerChat(player)
            .ToPlayerCenter(player);

        notifications.GRANTED_TO(player)
            .ToAllChat();
    }

    public void Revoke(CCSPlayerController player)
    {
        //  Player is already revoked
        if (!IsSpecialTreatment(player))
            return;

        _sts.Get(player).HasSpecialTreatment = false;

        this.SetSpecialColor(player, false);

        notifications.REVOKED
            .ToPlayerChat(player)
            .ToPlayerCenter(player);

        notifications.REVOKED_FROM(player)
            .ToAllChat();
    }

    private void SetSpecialColor(CCSPlayerController player, bool hasSt)
    {
        if (!player.IsValid || player.Pawn.Value == null)
            return;

        var color = hasSt
            ? Color.FromArgb(254, 0, 255, 0)
            : Color.FromArgb(0, 255, 0, 255);

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.Pawn.Value.Render = color;
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
    }
}