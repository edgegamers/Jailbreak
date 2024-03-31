using System.Drawing;
using System.Xml.Schema;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
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
    CCSPlayerController guard,
    IRaceLRMessages messages) : TeleportingRequest(plugin, manager, prisoner, guard)
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

        messages.END_RACE_INSTRUCTION.ToPlayerChat(prisoner);

        messages.RACE_STARTING_MESSAGE(prisoner).ToPlayerChat(guard);

        startLocation = prisoner.Pawn.Value.AbsOrigin.Clone();

        start = new BeamCircle(plugin, startLocation, 20, 16);
        start.SetColor(Color.Aqua);
        start.Draw();
    }

    // Called when the prisoner types !endrace
    public override void Execute()
    {
        state = LRState.Active;

        endLocation = prisoner.Pawn.Value.AbsOrigin.Clone();

        end = new BeamCircle(plugin, endLocation, 10, 32);
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
        var requiredDistance = getRequiredDistance();
        var requiredDistanceSqured = MathF.Pow(requiredDistance, 2);

        end?.SetRadius(requiredDistance / 2);
        end?.Update();

        var guardDist = guard.Pawn.Value.AbsOrigin.Clone().DistanceSquared(endLocation);

        if (guardDist < requiredDistanceSqured)
        {
            manager.EndLastRequest(this, LRResult.GuardWin);
            return;
        }

        var prisonerDist = prisoner.Pawn.Value.AbsOrigin.Clone().DistanceSquared(endLocation);
        if (prisonerDist < requiredDistanceSqured)
        {
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        }
    }

    // https://www.desmos.com/calculator/e1qwgpmtmz
    private float getRequiredDistance()
    {
        var elapsedSeconds = (DateTime.Now - raceStart).TotalSeconds;

        return (float)(10 + elapsedSeconds + Math.Pow(elapsedSeconds, 2.9) / 5000);
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
        raceTimer?.Kill();
        start?.Remove();
        end?.Remove();
    }
}