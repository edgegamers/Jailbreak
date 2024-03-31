using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.LastRequests;

public class Race(
    BasePlugin plugin,
    ILastRequestManager manager,
    CCSPlayerController prisoner,
    CCSPlayerController guard) : TeleportingRequest(plugin, manager, prisoner, guard)
{
    public override LRType type => LRType.Race;

    private BeamCircle? start, end;
    private Vector startLocation, endLocation;

    private Timer? raceTimer;
    private DateTime raceStart;

    public override void Setup()
    {
        base.Setup();

        prisoner.RemoveWeapons();

        guard.RemoveWeapons();
        guard.GiveNamedItem("weapon_knife");

        plugin.AddTimer(3, () =>
        {
            if (state != LRState.Pending)
                return;
            prisoner.GiveNamedItem("weapon_knife");
        });

        prisoner.PrintToCenter("Type !endrace to set the end point!");

        PrintToParticipants(prisoner.PlayerName +
                            " is starting a Race LR, pay attention to where they set the end point!");

        startLocation = prisoner.AbsOrigin!.Clone();

        start = new BeamCircle(plugin, startLocation, 10, 16);
        start.SetColor(Color.Aqua);
        start.Draw();
    }

    // Called when the prisoner types !endrace
    public override void Execute()
    {
        state = LRState.Active;

        endLocation = prisoner.AbsOrigin!.Clone();

        end = new BeamCircle(plugin, endLocation, 10, 16);
        end.SetColor(Color.Red);
        end.Draw();

        prisoner.Teleport(startLocation);
        guard.Teleport(startLocation);

        guard.Freeze();
        prisoner.Freeze();

        plugin.AddTimer(1, () =>
        {
            guard.UnFreeze();
            prisoner.UnFreeze();
        });

        raceStart = DateTime.Now;

        raceTimer = plugin.AddTimer(0.1f, Tick, TimerFlags.REPEAT);
    }

    private void Tick()
    {
        if (prisoner.AbsOrigin == null || guard.AbsOrigin == null)
            return;
        var requiredDistance = MathF.Pow(getRequiredDistance(), 2);

        end?.SetRadius(requiredDistance / 2);

        if (guard.AbsOrigin.DistanceSquared(endLocation) < requiredDistance)
        {
            manager.EndLastRequest(this, LRResult.GuardWin);
            return;
        }

        if (prisoner.AbsOrigin.DistanceSquared(endLocation) < requiredDistance)
        {
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        }
    }

    // https://www.desmos.com/calculator/e1qwgpmtmz
    private float getRequiredDistance()
    {
        var elapsedSeconds = (DateTime.Now - raceStart).TotalSeconds;

        return (float)(elapsedSeconds + Math.Pow(elapsedSeconds, 2.9) / 5000);
    }

    public override void OnEnd(LRResult result)
    {
        switch (result)
        {
            case LRResult.GuardWin:
                prisoner.Pawn.Value?.CommitSuicide(false, true);
                break;
            case LRResult.PrisonerWin:
                guard.Pawn.Value?.CommitSuicide(false, true);
                break;
        }

        state = LRState.Completed;
        start?.Remove();
        end?.Remove();
        raceTimer?.Kill();
    }
}