using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior : IPluginBehavior {
  private readonly IWardenService wardenService;
  private Vector? lastPosition;
  private BasePlugin? parent;

  public WardenPaintBehavior(IWardenService warden) { wardenService = warden; }

  public void Start(BasePlugin basePlugin) {
    parent = basePlugin;
    basePlugin.RegisterListener<Listeners.OnTick>(paint);
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

    new BeamLine(parent, start.Clone(), position.Clone()).Draw(30f);
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