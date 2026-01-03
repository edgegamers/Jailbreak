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

    var steam = w.SteamID;
    
    // Ensure settings are cached
    var settings = markerSettings.GetCachedSettings(steam);
    if (settings == null) {
      // Cache miss - load in background and use defaults for now
      Task.Run(async () => await markerSettings.EnsureCachedAsync(steam));
      settings = new MarkerSettings(BeamShapeType.CIRCLE, Color.White);
    }

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

        ensurePlacedMarker(settings.Value, MarkerPosition);
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

    ensurePlacedMarker(settings.Value, ping);
    placedMarker!.Move(ping);
    placedMarker.SetRadius(Radius);
    placedMarker.Update();

    API.Stats?.PushStat(new ServerStat("JB_MARKER",
      $"{ping.X:F2} {ping.Y:F2} {ping.Z:F2}"));

    locale.MarkerPlaced().ToAllChat();
    return HookResult.Handled;
  }

  private void ensurePlacedMarker(MarkerSettings settings, Vector pos) {
    // recreate if missing OR type changed
    if (placedMarker == null 
        || (placedMarker is { } existing 
            && !isCorrectType(existing, settings.Type))) {
      placedMarker?.Remove();
      placedMarker = factory.CreateShape(pos, settings.Type, Radius);
    }

    placedMarker.SetColor(settings.color);
  }

  private bool isCorrectType(BeamedPolylineShape shape, BeamShapeType type) {
    // Check if the shape matches the type - you may need to adjust this
    // based on your actual implementation
    return shape.GetType().Name.Contains(type.ToString());
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info)
    => warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
}