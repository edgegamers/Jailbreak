using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Extensions;

public static class CBaseEntityExtensions {
  public static void SetColor(this CBaseModelEntity? entity, Color color) {
    if (entity == null || !entity.IsValid) return;

    entity.RenderMode = RenderMode_t.kRenderTransColor;
    entity.Render     = color;
    Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");
  }
}