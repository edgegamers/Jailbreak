using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;

namespace Gangs.BaseImpl.Extensions;

public static class ColorExtensions {
  public static char GetChatColor(this Color? color) {
    if (color == Color.Orange) return ChatColors.Orange;
    if (color == Color.Yellow) return ChatColors.Yellow;
    if (color == Color.Green) return ChatColors.Green;
    if (color == Color.Cyan) return ChatColors.LightBlue;
    if (color == Color.Blue) return ChatColors.Blue;
    if (color == Color.Purple) return ChatColors.Purple;
    if (color == Color.Red) return ChatColors.Red;
    return ChatColors.White;
  }

  public static double GetColorMultiplier(this Color? color) {
    if (color == Color.Red) return 3;
    if (color == Color.Orange) return 1.5;
    if (color == Color.Yellow) return 1.25;
    if (color == Color.Green) return 1;
    if (color == Color.Cyan) return 2.5;
    if (color == Color.Blue) return 2;
    if (color == Color.Purple) return 2.25;
    if (color == null) return 5;
    return 1;
  }
}