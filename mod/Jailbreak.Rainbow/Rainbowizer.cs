using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Rainbow;

public class Rainbowizer : IRainbowColorizer {
  private readonly Dictionary<int, DateTime> rainbowTimers = new();
  private Timer? colorTimer;

  private BasePlugin parent = null!;

  public void Start(BasePlugin basePlugin, bool hotreload) {
    parent = basePlugin;
  }

  public void StartRainbow(CCSPlayerController player) {
    if (!player.IsValid) return;
    rainbowTimers[player.Slot] = DateTime.Now;
    colorTimer ??= parent.AddTimer(0.2f, tick, TimerFlags.REPEAT);
  }

  public void StopRainbow(int slot) {
    rainbowTimers.Remove(slot);
    if (rainbowTimers.Count != 0) return;
    colorTimer?.Kill();
    colorTimer = null;
  }

  private void tick() {
    foreach (var (slot, start) in rainbowTimers) {
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) {
        StopRainbow(slot);
        continue;
      }

      var dt = DateTime.Now - start;
      player.SetColor(calculateColor(dt));
    }
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    rainbowTimers.Clear();
    colorTimer?.Kill();

    return HookResult.Continue;
  }

  private Color calculateColor(TimeSpan dt) {
    var hue = dt.TotalSeconds % 360;
    return Color.FromArgb(255, ColorFromHSV(hue, 1, 1));
  }

  public static Color
    ColorFromHSV(double hue, double saturation, double value) {
    var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    var f  = hue / 60 - Math.Floor(hue / 60);

    value *= 255;
    var v = Convert.ToInt32(value);
    var p = Convert.ToInt32(value * (1 - saturation));
    var q = Convert.ToInt32(value * (1 - f * saturation));
    var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

    return hi switch {
      0 => Color.FromArgb(255, v, t, p),
      1 => Color.FromArgb(255, q, v, p),
      2 => Color.FromArgb(255, p, v, t),
      3 => Color.FromArgb(255, p, q, v),
      4 => Color.FromArgb(255, t, p, v),
      _ => Color.FromArgb(255, v, p, q)
    };
  }
}