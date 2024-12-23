using System.Drawing;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Draw;

public class TextSetting {
  public required string msg;
  public string fontName = "Arial";
  public bool enabled = true;
  public bool fullbright = true;
  public float worldUnitsPerPx = 0.4f;
  public float fontSize = 50;
  public float depthOffset = 0.0f;
  public Color color = Color.White;

  public PointWorldTextJustifyHorizontal_t horizontal =
    PointWorldTextJustifyHorizontal_t
     .POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;

  public PointWorldTextJustifyVertical_t vertical =
    PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;

  public PointWorldTextReorientMode_t reorient =
    PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
}