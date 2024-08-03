using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Views;

public interface ILogLocale {
  public IView BeginJailbreakLogs { get; }

  public IView EndJailbreakLogs { get; }

  public FormatObject Time() {
    var gamerules = ServerExtensions.GetGameRules();
    var start     = gamerules.RoundStartTime;
    var current   = Server.CurrentTime;
    var elapsed   = current - start;

    var minutes = Math.Floor(elapsed / 60f).ToString("00");
    var seconds = Math.Floor(elapsed % 60).ToString("00");

    return new StringFormatObject($"[{minutes}:{seconds}]", ChatColors.Gold);
  }

  public IView CreateLog(params FormatObject[] objects) {
    return new SimpleView { Time(), objects };
  }
}