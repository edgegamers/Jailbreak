﻿using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class PlayerExtensions {
  [Obsolete("Use CCSPlayerController#Team instead")]
  public static CsTeam GetTeam(this CCSPlayerController controller) {
    return (CsTeam)controller.TeamNum;
  }

  public static bool IsReal(this CCSPlayerController? player) {
    //  Do nothing else before this:
    //  Verifies the handle points to an entity within the global entity list.
    if (player == null) return false;
    if (!player.IsValid) return false;

    if (player.Connected != PlayerConnectedState.PlayerConnected) return false;

    if (player.IsHLTV) return false;

    return true;
  }

  public static void Teleport(this CCSPlayerController player,
    CCSPlayerController target) {
    if (!player.IsReal() || !target.IsReal()) return;

    var playerPawn = player.Pawn.Value;
    if (playerPawn == null || !playerPawn.IsValid) return;

    var targetPawn = target.Pawn.Value;
    if (targetPawn == null || !targetPawn.IsValid) return;

    if (targetPawn is { AbsRotation: not null, AbsOrigin: not null })
      Teleport(player, targetPawn.AbsOrigin, targetPawn.AbsRotation);
  }

  public static void Teleport(this CCSPlayerController player, Vector pos,
    QAngle? rot = null) {
    if (!player.IsReal()) return;

    var playerPawn = player.Pawn.Value;
    if (playerPawn == null) return;

    playerPawn.Teleport(pos, rot ?? playerPawn.AbsRotation!, new Vector());
  }

  public static void Freeze(this CCSPlayerController player) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;

    pawn.Freeze();
  }

  public static void UnFreeze(this CCSPlayerController player) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.UnFreeze();
  }

  public static void Freeze(this CBasePlayerPawn pawn) {
    if (!pawn.IsValid) return;
    pawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;

    Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 1);
    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
  }

  public static void UnFreeze(this CBasePlayerPawn pawn) {
    if (!pawn.IsValid) return;
    pawn.MoveType = MoveType_t.MOVETYPE_WALK;

    Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
  }

  public static void SetHealth(this CCSPlayerController player, int health) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.Health = health;
    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
  }

  public static void SetMaxHealth(this CCSPlayerController player, int health) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.MaxHealth = health;
    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
  }

  public static void SetArmor(this CCSPlayerController player, int armor) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.ArmorValue = armor;
    Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
  }

  public static void SetSpeed(this CCSPlayerController player, float speed) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.VelocityModifier = speed;
  }

  public static void
    SetGravity(this CCSPlayerController player, float gravity) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    pawn.GravityScale = gravity;
  }

  public static void SetColor(this CCSPlayerController player, Color color) {
    if (!player.IsValid) return;
    var pawn = player.PlayerPawn.Value;
    if (!player.IsValid || pawn == null || !pawn.IsValid) return;

    if (color.A == 255) color = Color.FromArgb(254, color.R, color.G, color.B);
    pawn.RenderMode = RenderMode_t.kRenderTransColor;
    pawn.Render     = color;
    Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
  }

  public static Color? GetColor(this CCSPlayerController player) {
    if (!player.IsValid) return null;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return null;

    return pawn.Render;
  }

  public static CBasePlayerWeapon? GetWeaponBase(
    this CCSPlayerController player, string designerName) {
    if (!player.IsValid) return null;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return null;

    return pawn.WeaponServices?.MyWeapons
     .FirstOrDefault(w => w.Value?.DesignerName == designerName)
    ?.Value;
  }
}