using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI;
using CS2ScreenMenuAPI.Enums;
using CS2ScreenMenuAPI.Internal;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Warden.Paint;
using MStatsShared;
using PostSelectAction = CS2ScreenMenuAPI.Enums.PostSelectAction;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden, IWardenLocale locale)
  : IPluginBehavior, IMarkerService {
  public static readonly FakeConVar<float> CV_MAX_RADIUS = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  public static readonly FakeConVar<float> CV_MIN_RADIUS = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  public static readonly FakeConVar<long> CV_RESIZE_TIME = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  private BeamCircle[] markers = [];
  private BeamCircle tmpMarker = null!;

  private readonly string[] markerNames = [
    ChatColors.Red + "Red", ChatColors.Green + "Green",
    ChatColors.Blue + "Blue", ChatColors.Purple + "Purple"
  ];

  private long placementTime;

  public Vector? MarkerPosition { get; private set; }
  public float radius { get; private set; }
  private bool activelyPlacing, removedMarker;

  private ScreenMenu menu = null!;
  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    tmpMarker = new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
      (int)Math.PI * 15);
    markers = [
      new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
        (int)Math.PI * 15),
      new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
        (int)Math.PI * 15),
      new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
        (int)Math.PI * 15),
      new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
        (int)Math.PI * 15)
    ];

    markers[0].SetColor(Color.Red);
    markers[1].SetColor(Color.Green);
    markers[2].SetColor(Color.Blue);
    markers[3].SetColor(Color.Purple);

    menu = new ScreenMenu("Markers", basePlugin) {
      PostSelectAction = PostSelectAction.Close,
      MenuType         = MenuType.KeyPress,
      PositionX        = -8.5f
    };

    menu.AddOption("Red", (_, _) => placeMarker(0));
    menu.AddOption("Green", (_, _) => placeMarker(1));
    menu.AddOption("Blue", (_, _) => placeMarker(2));
    menu.AddOption("Purple", (_, _) => placeMarker(3));

    basePlugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
    basePlugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  private void placeMarker(int index) {
    if (MarkerPosition == null) return;
    var marker = markers[index];
    marker.Move(MarkerPosition);
    marker.SetRadius(radius);
    marker.Update();
    tmpMarker.SetRadius(1);
    tmpMarker.Move(MarkerPosition);
    tmpMarker.Update();

    MarkerPosition = null;
    locale.MarkerPlaced(markerNames[index]).ToAllChat();
  }

  [GameEventHandler]
  public HookResult OnSpawn(EventPlayerSpawn spawn, GameEventInfo info) {
    if (spawn.Userid == null || !spawn.Userid.IsValid)
      return HookResult.Continue;
    SetBinds(spawn.Userid);
    return HookResult.Continue;
  }

  private void OnTick() {
    if (!warden.HasWarden) return;

    if (warden.Warden == null || !warden.Warden.IsReal()) return;
    if ((warden.Warden.Buttons & PlayerButtons.Attack2) == 0) {
      if (activelyPlacing && !removedMarker) {
        MenuAPI.CloseActiveMenu(warden.Warden);
        MenuAPI.OpenMenu(plugin, warden.Warden, menu);
      }

      activelyPlacing = false;
      removedMarker   = false;
      return;
    }

    var weapon = warden.Warden.Pawn.Value?.WeaponServices?.ActiveWeapon.Value
    ?.DesignerName;
    if (weapon != null && Tag.SNIPERS.Contains(weapon)
      || weapon == "weapon_sg556")
      return;

    var position = RayTrace.FindRayTraceIntersection(warden.Warden);
    if (position == null) return;

    if (!activelyPlacing) {
      for (var i = 0; i < markers.Length; i++) {
        var marker = markers[i];
        var dist   = marker.Position.DistanceSquared(position);
        if (dist < MathF.Pow(marker.Radius, 2)) {
          marker.SetRadius(0);
          marker.Update();
          locale.MarkerRemoved(markerNames[i]).ToAllChat();
          removedMarker = true;
          return;
        }
      }
    }

    if (removedMarker) return;

    if (MarkerPosition == null || !activelyPlacing) {
      tmpMarker.SetRadius(CV_MIN_RADIUS.Value);
      tmpMarker.Move(position);
      tmpMarker.Update();
      MarkerPosition  = position;
      activelyPlacing = true;
      placementTime   = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      API.Stats?.PushStat(new ServerStat("JB_MARKER",
        $"{position.X:F2} {position.Y:F2} {position.Z:F2}"));
      return;
    }

    var distance = MarkerPosition.Distance(position);
    distance = Math.Clamp(distance, CV_MIN_RADIUS.Value, CV_MAX_RADIUS.Value);

    radius = distance;
    tmpMarker.SetRadius(distance);
    tmpMarker.Update();
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing @event, GameEventInfo info) {
    var player = @event.Userid;

    if (!warden.IsWarden(player)) return HookResult.Handled;
    var vec = new Vector(@event.X, @event.Y, @event.Z);

    if (MarkerPosition != null) {
      var distance = MarkerPosition.Distance(vec);
      var timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        - placementTime;
      if (timeElapsed < CV_RESIZE_TIME.Value) {
        if (distance <= CV_MAX_RADIUS.Value * 1.3) {
          distance = Math.Clamp(distance, CV_MIN_RADIUS.Value,
            CV_MAX_RADIUS.Value);
          tmpMarker?.SetRadius(distance);
          tmpMarker?.Update();
          radius = distance;
          return HookResult.Handled;
        }
      } else if (distance <= radius) {
        tmpMarker?.Remove();
        return HookResult.Handled;
      }
    }

    radius         = CV_MIN_RADIUS.Value;
    MarkerPosition = vec;
    placementTime  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    API.Stats?.PushStat(new ServerStat("JB_MARKER",
      $"{vec.X:F2} {vec.Y:F2} {vec.Z:F2}"));
    tmpMarker?.Move(vec);
    tmpMarker?.SetRadius(radius);
    tmpMarker?.Update();
    return HookResult.Handled;
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info) {
    return warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
  }


  public static void SetBinds(CCSPlayerController player) {
    for (var i = 0; i < 10; i++)
      player.ExecuteClientCommand($"echo test | bind {i} \"slot{i};css_{i}\"");
  }
}