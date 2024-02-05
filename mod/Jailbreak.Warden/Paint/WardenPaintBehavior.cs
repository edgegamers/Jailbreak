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
    private Vector? _lastPosition;
    private readonly IWardenService _warden;
    private BasePlugin parent;

    public WardenPaintBehavior(IWardenService warden)
    {
        _warden = warden;
    }

    public void Start(BasePlugin parent)
    {
        this.parent = parent;
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

        new BeamLine(parent, start.Clone(), position.Clone()).Draw(10f);
    }

    private Vector? FindFloorIntersection(CCSPlayerController player)
    {
        if (player.Pawn.Value == null || player.PlayerPawn.Value == null)
            return null;
        var pawn = player.Pawn.Value;
        var playerPawn = player.PlayerPawn.Value;
        if (!pawn.IsValid || !playerPawn.IsValid || pawn.CameraServices == null)
            return null;

        var camera = pawn.CameraServices;
        var cameraOrigin = new Vector(pawn?.AbsOrigin?.X, pawn?.AbsOrigin?.Y,
            pawn.AbsOrigin.Z + camera.OldPlayerViewOffsetZ);
        var eye_angle = player.PlayerPawn.Value.EyeAngles;

        var pitch = Math.PI / 180 * eye_angle.X;
        var yaw = Math.PI / 180 * eye_angle.Y;

        // get direction vector from angles
        var eye_vector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)),
            (float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)-Math.Sin(pitch));

        return FindFloorIntersection(cameraOrigin, eye_vector, new Vector(eye_angle.X, eye_angle.Y, eye_angle.Z),
            pawn.AbsOrigin.Z);
    }

    private Vector? FindFloorIntersection(Vector start, Vector worldAngles, Vector rotationAngles, float z)
    {
        var pitch = rotationAngles.X; // 90 = straight down, -90 = straight up, 0 = straight ahead
        // normalize so 0 = straight down, 180 = straight up, 90 = straight ahead
        pitch = 90 - pitch;
        if (pitch >= 90)
            return null;
        var angle_a = ToRadians(90);
        var side_b = start.Z - z;
        var angle_c = ToRadians(pitch);


        var angle_b = 180 - 90 - pitch;
        var side_a = side_b * MathF.Sin(ToRadians(90)) / MathF.Sin(ToRadians(angle_b));
        var side_c = MathF.Sqrt(side_b * side_b + side_a * side_a - 2 * side_b * side_a * MathF.Cos(angle_c));

        var destination = start.Clone();
        destination.X += worldAngles.X * side_c;
        destination.Y += worldAngles.Y * side_c;
        destination.Z = z;
        return destination;
    }

    private static float ToRadians(float angle)
    {
        return (float)(Math.PI / 180) * angle;
    }
}