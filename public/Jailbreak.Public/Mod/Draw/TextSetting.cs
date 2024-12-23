using System.Drawing;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Draw;

public class TextSetting {
  public Color color = Color.White;
  public float depthOffset = 0.0f;
  public bool enabled = true;
  public string fontName = "Arial";
  public float fontSize = 50;
  public bool fullbright = true;

  public PointWorldTextJustifyHorizontal_t horizontal =
    PointWorldTextJustifyHorizontal_t
     .POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;

  public required string msg;

  public PointWorldTextReorientMode_t reorient =
    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;

  public PointWorldTextJustifyVertical_t vertical =
    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;

  public float worldUnitsPerPx = 0.4f;
}