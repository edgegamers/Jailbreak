using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;
using Jailbreak.Public.Mod.Warden;
using MStatsShared;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden, IWardenLocale locale,
  IBeamShapeFactory factory, IWardenMarkerSettings markerSettings)
  : IPluginBehavior, IMarkerService {
  public static readonly FakeConVar<float> CV_MAX_RADIUS = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  public static readonly FakeConVar<float> CV_MIN_RADIUS = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  public static readonly FakeConVar<long> CV_RESIZE_TIME = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  // placement state
  public Vector? MarkerPosition { get; private set; }
  public float Radius { get; private set; }
  private long placementTime;
  
  private BeamedPolylineShape? placedMarker;

  // cached warden settings
  private ulong? cachedWardenSteam;

  private MarkerSettings
    cachedSettings = new(BeamShapeType.CIRCLE, Color.White);

  private bool cacheReady;
  private bool cacheFetchInFlight;

  public void Start(BasePlugin basePlugin) {
    Radius = CV_MIN_RADIUS.Value;
    basePlugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing ev, GameEventInfo info) {
    var player = ev.Userid;
    if (!warden.IsWarden(player)) return HookResult.Handled;

    var w = warden.Warden;
    if (w == null || !w.IsReal()) return HookResult.Handled;

    ensureWardenMarkerCached(w);

    var settings = cacheReady ?
      cachedSettings :
      new MarkerSettings(BeamShapeType.CIRCLE, Color.White);

    var ping = new Vector(ev.X, ev.Y, ev.Z);
    var now  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // If we already have an active marker center, allow resize within window
    if (MarkerPosition != null) {
      var elapsed = now - placementTime;

      if (elapsed < CV_RESIZE_TIME.Value) {
        var distance = MarkerPosition.Distance(ping);
        
        if (!(distance <= CV_MAX_RADIUS.Value * 1.3f))
          return HookResult.Handled;
        distance = Math.Clamp(distance, CV_MIN_RADIUS.Value,
          CV_MAX_RADIUS.Value);
        Radius = distance;

        ensurePlacedMarker(settings, MarkerPosition);
        placedMarker!.SetRadius(Radius);
        placedMarker.Update();

        return HookResult.Handled;
      }

      // timeout: next ping starts a NEW marker
      MarkerPosition = null;
    }
    
    MarkerPosition = ping;
    Radius         = CV_MIN_RADIUS.Value;
    placementTime  = now;

    ensurePlacedMarker(settings, ping);
    placedMarker!.Move(ping);
    placedMarker.SetRadius(Radius);
    placedMarker.Update();

    API.Stats?.PushStat(new ServerStat("JB_MARKER",
      $"{ping.X:F2} {ping.Y:F2} {ping.Z:F2}"));

    locale.MarkerPlaced("marker").ToAllChat();
    return HookResult.Handled;
  }

  private void ensurePlacedMarker(MarkerSettings settings, Vector pos) {
    // recreate if missing OR warden changed type since last cached setting
    if (placedMarker == null || settings.Type != cachedSettings.Type) {
      placedMarker?.Remove();
      placedMarker = factory.CreateShape(pos, settings.Type, Radius);
    }

    placedMarker.SetColor(settings.color);
  }

  private void ensureWardenMarkerCached(CCSPlayerController w) {
    var steam = w.SteamID;

    if (cachedWardenSteam == steam && cacheReady) return;

    if (cachedWardenSteam != steam) {
      cachedWardenSteam  = steam;
      cacheReady         = false;
      cacheFetchInFlight = false;
    }

    if (cacheFetchInFlight) return;
    cacheFetchInFlight = true;

    Task.Run(async () => {
      var settings = await markerSettings.GetForWardenAsync(steam);

      await Server.NextFrameAsync(() => {
        cacheFetchInFlight = false;

        if (!warden.HasWarden || warden.Warden == null) return;
        if (warden.Warden.SteamID != steam) return;

        cachedSettings = settings;
        cacheReady     = true;
      });
    });
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info)
    => warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
}