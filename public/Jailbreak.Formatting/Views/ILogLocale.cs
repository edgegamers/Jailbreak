using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Public.Utils;

namespace Jailbreak.Formatting.Views;

public interface ILogLocale {
  public IView BeginJailbreakLogs { get; }

  public IView EndJailbreakLogs { get; }

  public FormatObject Time() {
    var elapsed = RoundUtil.GetTimeElapsed();

    var minutes = Math.Floor(elapsed / 60f).ToString("00");
    var seconds = (elapsed % 60).ToString("00");

    return new StringFormatObject($"[{minutes}:{seconds}]", ChatColors.Gold);
  }

  public IView CreateLog(params FormatObject[] objects) {
    return new SimpleView { Time(), objects };
  }
}