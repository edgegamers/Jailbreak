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
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Warden.Enums;
using Jailbreak.Validator;
using MStatsShared;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden, IWardenLocale locale,
  IWardenMarkerSettings markerSettings) : IPluginBehavior, IMarkerService {
  public static readonly FakeConVar<float> CV_MAX_RADIUS = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  public static readonly FakeConVar<float> CV_MIN_RADIUS = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  public static readonly FakeConVar<long> CV_RESIZE_TIME = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  public static readonly FakeConVar<int> CV_PARTICLE_COUNT = new(
    "css_jb_warden_particle_count",
    "Number of particle used in drawing the markers", 33,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(16, 256));

  // placement state
  public Vector? MarkerPosition { get; private set; }
  public float Radius { get; private set; }

  private long placementTime;
  private Marker? placedMarker;

  public void Start(BasePlugin basePlugin) {
    Radius = CV_MIN_RADIUS.Value;

    // Register all marker shapes so CS2Draw can resolve them
    if (API.Draw != null)
      foreach (var shape in MarkerShapeTypeExtensions.All())
        API.Draw.RegisterShape(new MarkerShapeSetup(shape, CV_MIN_RADIUS.Value,
          CV_PARTICLE_COUNT.Value));

    basePlugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing ev, GameEventInfo info) {
    var player = ev.Userid;
    if (!warden.IsWarden(player)) return HookResult.Handled;

    var w = warden.Warden;
    if (w == null || !w.IsReal()) return HookResult.Handled;

    var steam    = w.SteamID;
    var settings = markerSettings.GetCachedSettings(steam);

    if (settings == null) {
      Task.Run(async () => await markerSettings.EnsureCachedAsync(steam));
      settings = new MarkerSettings(MarkerShapeType.CIRCLE, Color.White);
    }

    var ping = new Vector(ev.X, ev.Y, ev.Z + 4);
    var now  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // within resize window — update radius and re-place at same center
    if (MarkerPosition != null) {
      var elapsed = now - placementTime;

      if (elapsed < CV_RESIZE_TIME.Value) {
        var distance = MarkerPosition.Distance(ping);

        if (!(distance <= CV_MAX_RADIUS.Value * 1.3f))
          return HookResult.Handled;

        Radius = Math.Clamp(distance, CV_MIN_RADIUS.Value, CV_MAX_RADIUS.Value);

        ensurePlacedMarker(settings.Value, MarkerPosition);
        placedMarker!.Resize(Radius);

        return HookResult.Handled;
      }

      // resize window expired — next ping starts a fresh marker
      MarkerPosition = null;
    }

    MarkerPosition = ping;
    Radius         = CV_MIN_RADIUS.Value;
    placementTime  = now;

    ensurePlacedMarker(settings.Value, ping);
    placedMarker!.Move(ping).Resize(Radius);

    API.Stats?.PushStat(new ServerStat("JB_MARKER",
      $"{ping.X:F2} {ping.Y:F2} {ping.Z:F2}"));

    locale.MarkerPlaced().ToChat(w);
    return HookResult.Handled;
  }

  // Recreates the marker only if it doesn't exist or the shape type changed.
  // Otherwise just updates the color.
  private void ensurePlacedMarker(MarkerSettings settings, Vector pos) {
    if (API.Draw == null) return;

    if (placedMarker == null || placedMarker.ShapeType != settings.Type) {
      placedMarker?.Cancel();
      placedMarker = new Marker(API.Draw, pos, settings.Type, Radius)
       .Color(settings.color)
       .Infinite();
      placedMarker.Place();
      return;
    }

    placedMarker.Recolor(settings.color);
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info)
    => warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
}