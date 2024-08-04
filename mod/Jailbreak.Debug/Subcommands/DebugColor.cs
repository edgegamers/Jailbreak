using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Debug.Subcommands;

public class DebugColor(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (info.ArgCount == 1 || executor == null) {
      info.ReplyToCommand("css_color [<alpha>| [red] [green] [blue]]");
      return;
    }

    var alpha = 255;
    var red   = 255;
    var green = 255;
    var blue  = 255;

    switch (info.ArgCount) {
      case 2:
        int.TryParse(info.GetArg(1), out alpha);
        break;
      case <= 4:
        info.ReplyToCommand("Invalid number of arguments.");
        break;
      default: {
        var currentColor = executor.GetColor();
        if (currentColor == null) {
          info.ReplyToCommand("Failed to get color.");
          return;
        }

        red   = currentColor.Value.R;
        green = currentColor.Value.G;
        blue  = currentColor.Value.B;
        break;
      }
    }

    if (info.ArgCount > 3) {
      int.TryParse(info.GetArg(info.ArgCount - 3), out red);
      int.TryParse(info.GetArg(info.ArgCount - 2), out green);
      int.TryParse(info.GetArg(info.ArgCount - 1), out blue);
    }

    executor.SetColor(System.Drawing.Color.FromArgb(alpha, red, green, blue));
    executor.PrintToChat($"DebugColor set to {alpha} {red} {green} {blue}");
  }
}