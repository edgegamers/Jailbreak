using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Teams;

public class RebelManager : IPluginBehavior, IRebelService
{
    private Dictionary<CCSPlayerController, float> rebelTimes = new();

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        
        parent.AddTimer(1f, () =>
        {
            foreach (var player in GetActiveRebels())
            {
                if (!player.IsValid)
                    continue;

                if (GetRebelTimeLeft(player) <= 0)
                {
                    UnmarkRebel(player);
                    continue;
                }

                ApplyRebelColor(player);
            }
        });
    }

    HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (rebelTimes.ContainsKey(@event.Userid))
        {
            rebelTimes.Remove(@event.Userid);
        }

        return HookResult.Continue;
    }

    public ISet<CCSPlayerController> GetActiveRebels()
    {
        return rebelTimes.Keys.ToHashSet();
    }

    public float GetRebelTimeLeft(CCSPlayerController player)
    {
        if (rebelTimes.TryGetValue(player, out float time))
        {
            return time - DateTime.Now.Ticks / 1000f;
        }

        return 0;
    }

    public bool MarkRebel(CCSPlayerController player, float time)
    {
        rebelTimes.Add(player, DateTime.Now.Ticks / 1000f + time);
        ApplyRebelColor(player);
        return true;
    }

    public void UnmarkRebel(CCSPlayerController player)
    {
        player.PrintToChat("You are no longer a rebel");
        rebelTimes.Remove(player);
        ApplyRebelColor(player);
    }

    // https://www.desmos.com/calculator/g2v6vvg7ax 
    private float GetRebelTimePercentage(CCSPlayerController player)
    {
        float x = GetRebelTimeLeft(player);
        if (x > 120)
            return 1;
        if (x <= 0)
            return 0;
        return (float)(100 - (120 - x) * (Math.Sqrt(120 - x)) / 13f) / 100;
    }

    private void ApplyRebelColor(CCSPlayerController player)
    {
        if (!player.IsValid || player.Pawn.Value == null)
            return;
        var percentage = GetRebelTimePercentage(player);
        var inverse = 1 - percentage;
        var inverseInt = (int)(inverse * 255);
        var color = Color.FromArgb(254, (int)percentage * 255, inverseInt, inverseInt);
        if (percentage <= 0)
        {
            color = Color.FromArgb(254, 255, 255, 255);
        }

        player.Pawn.Value.Render = color;
    }
}