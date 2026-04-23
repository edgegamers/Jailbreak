using System.Diagnostics;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CS2DrawShared.Timers;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;
using RayTraceAPI;
using WardenPaintColorPerk;
using TraceOptions = RayTraceAPI.TraceOptions;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior(IWardenService wardenService,
  IServiceProvider provider) : IPluginBehavior {
  private readonly IRainbowColorizer? colorizer =
    provider.GetService<IRainbowColorizer>();

  // per-color cache
  private WardenPaintColor?[] colors = new WardenPaintColor?[65];
  private bool[] fetched = new bool[65];

  // trail state
  private CCSPlayerController? painter;
  private ILoopTimer? trailTimer;
  private CInfoTarget? trailAnchor;
  private int lastActiveTrailTick;
  private bool wasHoldingLastTick;
  private static readonly Vector EYE_OFFSET = new(0, 0, 64.09f);
  private const int TRAIL_SPLIT_TIMEOUT = 128;

  public void Start(BasePlugin basePlugin) {
    painter = wardenService.Warden;
    basePlugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    colors  = new WardenPaintColor?[65];
    fetched = new bool[65];
    stopTrail();
    return HookResult.Continue;
  }

  private void OnTick() {
    // keep painter in sync with current warden
    painter = wardenService.Warden;

    if (painter?.PlayerPawn.Value == null) return;

    // player released Use — stop the trail
    if ((painter.Buttons & PlayerButtons.Use) == 0) {
      stopTrail();
      return;
    }

    var painterPawn = painter.PlayerPawn.Value;
    if (painterPawn == null) return;
    if (painterPawn.AbsOrigin == null) return;
    var absOrigin = painterPawn.AbsOrigin;
    var eyePosition = new Vector(absOrigin.X, absOrigin.Y,
      absOrigin.Z + painterPawn.ViewOffset.Z);
    var eyeAngles = painterPawn.EyeAngles;

    var forward = new Vector();
    NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, 0, 0);
    var endOrigin = new Vector(eyePosition.X + forward.X * 8192,
      eyePosition.Y + forward.Y * 8192, eyePosition.Z + forward.Z * 8192);

    var options = new TraceOptions {
      DrawBeam      = 0,
      InteractsWith = (ulong)InteractionLayers.MASK_BRUSH_ONLY,
      InteractsExclude =
        (ulong)(InteractionLayers.Player | InteractionLayers.NoDraw),
    };

    var now = Server.TickCount;
    if (!API.RayTrace!.TraceEndShape(eyePosition, endOrigin, painterPawn,
      options, out var result)) { return; }

    if (!result.DidHit) { return; }

    var pos = result.EndPos.ToCsVector();
    pos.Z += 5f;

    var isFirstHold = !wasHoldingLastTick;
    var playerPaused = lastActiveTrailTick > 0
      && now - lastActiveTrailTick > TRAIL_SPLIT_TIMEOUT;
    var anchorLost = trailAnchor == null;

    if (isFirstHold || playerPaused || anchorLost) startNewTrailSegment(pos);

    trailAnchor?.Teleport(pos, QAngle.Zero, Vector.Zero);

    wasHoldingLastTick  = true;
    lastActiveTrailTick = now;
  }

  private void startNewTrailSegment(Vector pos) {
    stopTrail();

    trailAnchor = Utilities.CreateEntityByName<CInfoTarget>("info_target");
    if (trailAnchor == null) return;

    trailAnchor.Teleport(pos, QAngle.Zero, Vector.Zero);
    trailAnchor.DispatchSpawn();

    if (painter == null) return;

    var color = getColor(painter);

    trailTimer = API.Draw?.Trail(trailAnchor).Color(color).Start();
  }

  private void stopTrail() {
    trailTimer?.Stop();
    trailTimer = null;

    trailAnchor?.Remove();
    trailAnchor = null;

    wasHoldingLastTick = false;
  }

  // ── Color resolution ──────────────────────────────────────────────────────

  private Color getColor(CCSPlayerController player) {
    if (player.Pawn.Value == null || player.PlayerPawn.Value == null)
      return Color.White;

    var pawn = player.Pawn.Value;
    if (pawn == null || !pawn.IsValid) return Color.White;

    var color = colors[player.Index];
    if (color != null) {
      if (color == WardenPaintColor.RAINBOW)
        return colorizer?.GetRainbow() ?? Color.White;
      if ((color & WardenPaintColor.RANDOM) != 0)
        return color.Value.PickRandom() ?? Color.White;
      return color.Value.GetColor() ?? Color.White;
    }

    if (fetched[player.Index]) return Color.White;
    fetched[player.Index] = true;

    var wrapper = new PlayerWrapper(player);
    Task.Run(async () => {
      color                = await fetchColor(wrapper);
      colors[player.Index] = color;
    });

    return Color.White;
  }

  private async Task<WardenPaintColor> fetchColor(PlayerWrapper player) {
    var gangs       = API.Gangs?.Services.GetService<IGangManager>();
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();

    if (gangs == null || playerStats == null || players == null
      || gangStats == null)
      return WardenPaintColor.DEFAULT;

    var playerColors = await playerStats.GetForPlayer<WardenPaintColor>(
      player.Steam, WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    var gangPlayer = await players.GetPlayer(player.Steam);
    if (gangPlayer?.GangId == null) return WardenPaintColor.DEFAULT;

    var gang = await gangs.GetGang(gangPlayer.GangId.Value);
    if (gang == null) return WardenPaintColor.DEFAULT;

    var available = await gangStats.GetForGang<WardenPaintColor>(gang,
      WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    if (playerColors == WardenPaintColor.RANDOM)
      return playerColors | available;

    return playerColors & available;
  }
}