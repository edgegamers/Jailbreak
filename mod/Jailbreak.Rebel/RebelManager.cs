using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;
using MStatsShared;

namespace Jailbreak.Rebel;

public class RebelManager(IRebelLocale notifs, IRichLogService logs)
  : IPluginBehavior, IRebelService {
  [Obsolete("No longer used, use FakeConvar")]
  public static readonly int MAX_REBEL_TIME = 45;

  public static readonly FakeConVar<int> CV_REBEL_TIME =
    new("css_jb_rebel_time", "Time to mark a rebel for", 45,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 500));

  private readonly Dictionary<CCSPlayerController, long> rebelTimes = new();
  private bool enabled = true;

  public void Start(BasePlugin basePlugin) {
    basePlugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    basePlugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    basePlugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);

    basePlugin.AddTimer(1f, () => {
      foreach (var player in GetActiveRebels()) {
        if (!player.IsValid
          || player.Connected != PlayerConnectedState.PlayerConnected)
          continue;

        if (GetRebelTimeLeft(player) <= 0) {
          UnmarkRebel(player);
          continue;
        }

        applyRebelColor(player);
      }
    }, TimerFlags.REPEAT);
  }

  public ISet<CCSPlayerController> GetActiveRebels() {
    return rebelTimes.Keys.ToHashSet();
  }

  public long GetRebelTimeLeft(CCSPlayerController player) {
    if (rebelTimes.TryGetValue(player, out var time))
      return time - DateTimeOffset.Now.ToUnixTimeSeconds();

    return 0;
  }

  public bool MarkRebel(CCSPlayerController player, long time = -1) {
    if (!enabled) return false;
    if (!rebelTimes.ContainsKey(player))
      logs.Append(logs.Player(player), "is now a rebel.");

    var pos = player.Pawn.Value?.AbsOrigin;
    if (pos != null)
      API.Stats?.PushStat(new ServerStat("JB_REBEL_STARTED",
        $"{player.SteamID} {pos.X:F2} {pos.Y:F2} {pos.Z:F2}"));

    if (time == -1) time = CV_REBEL_TIME.Value;

    rebelTimes[player] = DateTimeOffset.Now.ToUnixTimeSeconds() + time;
    applyRebelColor(player);
    return true;
  }

  public void UnmarkRebel(CCSPlayerController player) {
    if (rebelTimes.ContainsKey(player)) {
      notifs.NoLongerRebel.ToChat(player);
      logs.Append(logs.Player(player), "is no longer a rebel.");
    }

    rebelTimes.Remove(player);
    applyRebelColor(player);
  }

  public void DisableRebelForRound() { enabled = false; }

  private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    enabled = true;
    rebelTimes.Clear();
    foreach (var player in Utilities.GetPlayers()) applyRebelColor(player);

    return HookResult.Continue;
  }

  private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    if (@event.Userid == null) return HookResult.Continue;
    if (rebelTimes.ContainsKey(@event.Userid)) rebelTimes.Remove(@event.Userid);

    return HookResult.Continue;
  }

  private HookResult OnPlayerDeath(EventPlayerDeath @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;
    if (!player.IsReal()) return HookResult.Continue;
    rebelTimes.Remove(player);
    return HookResult.Continue;
  }

  // https://www.desmos.com/calculator/g2v6vvg7ax
  private float getRebelTimePercentage(CCSPlayerController player) {
    var x = GetRebelTimeLeft(player);
    if (x > CV_REBEL_TIME.Value) return 1;
    if (x <= 0) return 0;
    return (float)(100 - (CV_REBEL_TIME.Value - x)
      * Math.Sqrt(CV_REBEL_TIME.Value - x) / 3.8f) / 100;
  }

  private Color getRebelColor(CCSPlayerController player) {
    var percent             = getRebelTimePercentage(player);
    var percentRgb          = 255 - (int)Math.Round(percent * 255.0);
    var color               = Color.FromArgb(254, 255, percentRgb, percentRgb);
    if (percent <= 0) color = Color.FromArgb(254, 255, 255, 255);

    return color;
  }

  private void applyRebelColor(CCSPlayerController player) {
    if (!player.IsReal() || player.Pawn.Value == null) return;
    var color = getRebelColor(player);

    player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
    player.Pawn.Value.Render     = color;
    Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity",
      "m_clrRender");

    player.ColorScreen(
      Color.FromArgb(8 + (int)Math.Round(getRebelTimePercentage(player) * 32),
        Color.Red), 1f, 1.5f, PlayerExtensions.FadeFlags.FADE_OUT);
  }
}