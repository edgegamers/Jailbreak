using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Rebel;

public class RebelManager(IRebelNotifications notifs, IRichLogService logs)
  : IPluginBehavior, IRebelService {
  public static readonly int MAX_REBEL_TIME = 45;
  private readonly Dictionary<CCSPlayerController, long> rebelTimes = new();

  public void Start(BasePlugin basePlugin) {
    basePlugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    basePlugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    basePlugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
    basePlugin.RegisterListener<Listeners.OnTick>(OnTick);

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
        sendTimeLeft(player);
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

  public bool MarkRebel(CCSPlayerController player, long time = 30) {
    if (!rebelTimes.ContainsKey(player))
      logs.Append(logs.Player(player), "is now a rebel.");

    rebelTimes[player] = DateTimeOffset.Now.ToUnixTimeSeconds() + time;
    applyRebelColor(player);
    return true;
  }

  public void UnmarkRebel(CCSPlayerController player) {
    if (rebelTimes.ContainsKey(player)) {
      notifs.NoLongerRebel.ToPlayerChat(player);
      logs.Append(logs.Player(player), "is no longer a rebel.");
    }

    rebelTimes.Remove(player);
    applyRebelColor(player);
  }

  private void OnTick() {
    foreach (var player in GetActiveRebels()) {
      if (!player.IsReal()) continue;

      if (GetRebelTimeLeft(player) <= 0) continue;

      sendTimeLeft(player);
    }
  }

  private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
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
    if (x > MAX_REBEL_TIME) return 1;
    if (x <= 0) return 0;
    return (float)(100 - (MAX_REBEL_TIME - x) * Math.Sqrt(MAX_REBEL_TIME - x)
      / 3.8f) / 100;
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
  }

  private void sendTimeLeft(CCSPlayerController player) {
    // var timeLeft = GetRebelTimeLeft(player);
    // var formattedTime = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
    var color = getRebelColor(player);
    var formattedColor =
      $"<font color=\"#{color.R:X2}{color.G:X2}{color.B:X2}\">";

    player.PrintToCenterHtml(
      $"You are {formattedColor}<b>rebelling</b></font>");
  }
}