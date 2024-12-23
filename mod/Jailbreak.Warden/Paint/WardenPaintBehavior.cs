using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Rainbow;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;
using WardenPaintColorPerk;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior(IWardenService wardenService,
  IServiceProvider provider) : IPluginBehavior {
  private Vector? lastPosition;
  private BasePlugin? parent;

  private IRainbowColorizer? colorizer =
    provider.GetService<IRainbowColorizer>();

  private WardenPaintColor?[] colors = new WardenPaintColor?[65];

  public void Start(BasePlugin basePlugin) {
    parent = basePlugin;
    basePlugin.RegisterListener<Listeners.OnTick>(paint);
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    colors = new WardenPaintColor?[65];
    return HookResult.Continue;
  }

  private void paint() {
    if (!wardenService.HasWarden) return;

    var warden = wardenService.Warden;
    if (warden == null || !warden.IsReal()) return;

    if ((warden.Buttons & PlayerButtons.Use) == 0) return;

    var position = findFloorIntersection(warden);
    if (position == null) return;

    var start = lastPosition ?? position;
    start = start.Clone();

    if (lastPosition != null
      && position.DistanceSquared(lastPosition) < 25 * 25)
      return;

    lastPosition = position;
    if (start.DistanceSquared(position) > 150 * 150) start = position;

    if (parent == null)
      throw new NullReferenceException("Parent plugin is null");

    var color = getColor(warden);
    var line  = new BeamLine(parent, start.Clone(), position.Clone());
    line.SetColor(color);
    line.Draw(30);
  }

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

    var (success, playerColors) =
      await playerStats.GetForPlayer<WardenPaintColor>(player.Steam,
        WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    if (!success) return WardenPaintColor.DEFAULT;

    var gangPlayer = await players.GetPlayer(player.Steam);

    if (gangPlayer?.GangId == null) return WardenPaintColor.DEFAULT;

    var gang = await gangs.GetGang(gangPlayer.GangId.Value);
    if (gang == null) return WardenPaintColor.DEFAULT;

    var (_, available) = await gangStats.GetForGang<WardenPaintColor>(gang,
      WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    return playerColors & available;
  }

  private Vector? findFloorIntersection(CCSPlayerController player) {
    if (player.Pawn.Value == null || player.PlayerPawn.Value == null)
      return null;
    var pawn       = player.Pawn.Value;
    var playerPawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid || !playerPawn.IsValid
      || pawn.CameraServices == null)
      return null;
    if (pawn.AbsOrigin == null) return null;

    var camera = pawn.CameraServices;
    var cameraOrigin = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin!.Y,
      pawn.AbsOrigin!.Z + camera.OldPlayerViewOffsetZ);
    var eyeAngle = player.PlayerPawn.Value.EyeAngles;

    var pitch = Math.PI / 180 * eyeAngle.X;
    var yaw   = Math.PI / 180 * eyeAngle.Y;

    // get direction vector from angles
    var eyeVector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)),
      (float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)-Math.Sin(pitch));

    return findFloorIntersection(cameraOrigin, eyeVector,
      new Vector(eyeAngle.X, eyeAngle.Y, eyeAngle.Z), pawn.AbsOrigin.Z);
  }

  private Vector? findFloorIntersection(Vector start, Vector worldAngles,
    Vector rotationAngles, float z) {
    var pitch =
      rotationAngles
       .X; // 90 = straight down, -90 = straight up, 0 = straight ahead
    // normalize so 0 = straight down, 180 = straight up, 90 = straight ahead
    pitch = 90 - pitch;
    if (pitch >= 90) return null;

    // var angleA = ToRadians(90);
    var sideB  = start.Z - z;
    var angleC = toRadians(pitch);


    var angleB = 180 - 90 - pitch;
    var sideA = sideB * MathF.Sin(toRadians(90)) / MathF.Sin(toRadians(angleB));
    var sideC = MathF.Sqrt(sideB * sideB + sideA * sideA
      - 2 * sideB * sideA * MathF.Cos(angleC));

    var destination = start.Clone();
    destination.X += worldAngles.X * sideC;
    destination.Y += worldAngles.Y * sideC;
    destination.Z =  z;
    return destination;
  }

  private static float toRadians(float angle) {
    return (float)(Math.PI / 180) * angle;
  }
}