using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior : IPluginBehavior
{
    private readonly IWardenService _warden;
    private Vector? _lastPosition;
    private BasePlugin? _parent;

    public WardenPaintBehavior(IWardenService warden)
    {
        _warden = warden;
    }

    public void Start(BasePlugin parent)
    {
        _parent = parent;
        parent.RegisterListener<Listeners.OnTick>(Paint);
    }

    private void Paint()
    {
        if (!_warden.HasWarden)
            return;

        var warden = _warden.Warden;
        if (warden == null || !warden.IsReal())
            return;

        if ((warden.Buttons & PlayerButtons.Use) == 0) return;

        var position = FindFloorIntersection(warden);
        if (position == null)
            return;

        var start = _lastPosition ?? position;
        start = start.Clone();

        if (_lastPosition != null && position.DistanceSquared(_lastPosition) < 25 * 25) return;

        _lastPosition = position;
        if (start.DistanceSquared(position) > 150 * 150) start = position;

        if (_parent == null)
            throw new NullReferenceException("Parent plugin is null");

        new BeamLine(_parent, start.Clone(), position.Clone()).Draw(30f);
    }

    private Vector? FindFloorIntersection(CCSPlayerController player)
    {
        if (player.Pawn.Value == null || player.PlayerPawn.Value == null)
            return null;
        var pawn = player.Pawn.Value;
        var playerPawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid || !playerPawn.IsValid || pawn.CameraServices == null)
            return null;
        if (pawn.AbsOrigin == null)
            return null;

        var camera = pawn.CameraServices;
        var cameraOrigin = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin!.Y,
            pawn.AbsOrigin!.Z + camera.OldPlayerViewOffsetZ);
        var eyeAngle = player.PlayerPawn.Value.EyeAngles;

        var pitch = Math.PI / 180 * eyeAngle.X;
        var yaw = Math.PI / 180 * eyeAngle.Y;

        // get direction vector from angles
        var eyeVector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)),
            (float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)-Math.Sin(pitch));

        return FindFloorIntersection(cameraOrigin, eyeVector, new Vector(eyeAngle.X, eyeAngle.Y, eyeAngle.Z),
            pawn.AbsOrigin.Z);
    }

    private Vector? FindFloorIntersection(Vector start, Vector worldAngles, Vector rotationAngles, float z)
    {
        var pitch = rotationAngles.X; // 90 = straight down, -90 = straight up, 0 = straight ahead
        // normalize so 0 = straight down, 180 = straight up, 90 = straight ahead
        pitch = 90 - pitch;
        if (pitch >= 90)
            return null;

        // var angleA = ToRadians(90);
        var sideB = start.Z - z;
        var angleC = ToRadians(pitch);


        var angleB = 180 - 90 - pitch;
        var sideA = sideB * MathF.Sin(ToRadians(90)) / MathF.Sin(ToRadians(angleB));
        var sideC = MathF.Sqrt(sideB * sideB + sideA * sideA - 2 * sideB * sideA * MathF.Cos(angleC));

        var destination = start.Clone();
        destination.X += worldAngles.X * sideC;
        destination.Y += worldAngles.Y * sideC;
        destination.Z = z;
        return destination;
    }

    private static float ToRadians(float angle)
    {
        return (float)(Math.PI / 180) * angle;
    }
}