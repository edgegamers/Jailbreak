using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior : IPluginBehavior
{
    private IWardenService _warden;
    private BasePlugin parent;

    public WardenPaintBehavior(IWardenService warden)
    {
        _warden = warden;
    }

    public void Start(BasePlugin parent)
    {
        this.parent = parent;
        parent.AddTimer(0.5f, Paint, TimerFlags.REPEAT);
    }

    private void Paint()
    {
        if (!_warden.HasWarden)
            return;
        var warden = _warden.Warden;
        if (warden == null || !warden.IsValid)
            return;

        if ((warden.Buttons & PlayerButtons.Use) == 0)
            return;
        if (warden.Pawn.Value == null || warden.PlayerPawn.Value == null)
            return;
        CBasePlayerPawn pawn = warden.Pawn.Value;
        CCSPlayerPawn playerPawn = warden.PlayerPawn.Value;
        if (!pawn.IsValid || !playerPawn.IsValid || pawn.CameraServices == null)
            return;

        CPlayer_CameraServices camera = pawn.CameraServices;

        var start = pawn.LookTargetPosition;

        new BeamCircle(parent, start, 40, 5).Draw(15f);

        Vector cameraOrigin = new Vector(pawn?.AbsOrigin?.X, pawn?.AbsOrigin?.Y,
            pawn.AbsOrigin.Z + camera.OldPlayerViewOffsetZ);
        QAngle eye_angle = warden.PlayerPawn.Value.EyeAngles;
        double pitch = (Math.PI / 180) * eye_angle.X;
        double yaw = (Math.PI / 180) * eye_angle.Y;
        Vector eye_vector = new Vector((float)(Math.Cos(pitch) * Math.Cos(yaw)),
            (float)(Math.Cos(pitch) * Math.Sin(yaw)), (float)(-Math.Sin(pitch)));

        start = FindFloorIntersection(cameraOrigin, eye_vector, pawn.AbsOrigin.Z);
        if (start == null)
            return;
        var circle = new BeamCircle(parent, start, 40, 5);
        circle.SetColor(Color.Red);
        circle.Draw(15f);
    }

    private Vector? FindFloorIntersection(Vector start, Vector angle, float z)
    {
        float pitch = angle.X; // 90 = straight down, -90 = straight up
        // normalize so 0 = straight down, 180 = straight up
        pitch = (pitch + 270) % 360;
        if (pitch > 180)
            pitch -= 180;
        if (pitch >= 90)
            return null;
        float angle_a = 90;
        float side_b = z;
        float angle_c = pitch;

        float side_a = (float)(angle_c * Math.Sin(angle_a) / Math.Sin(180 - angle_a - angle_c));

        Vector destination = start.Clone();
        destination.Add(angle.Clone().Normalize().Scale(side_a));
        return destination;
    }
}