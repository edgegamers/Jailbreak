using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jailbreak.Public.Mod.LastRequest.Enums;

public enum LRType
{
    GunToss,
    RockPaperScissors,
    KnifeFight,
    NoScope,
    FiftyFifty,
    ShotForShot,
    MagForMag,
    Race
}

public static class LRTypeExtensions
{
    public static string ToFriendlyString(this LRType type)
    {
        return type switch
        {
            LRType.GunToss => "Gun Toss",
            LRType.RockPaperScissors => "Rock Paper Scissors",
            LRType.KnifeFight => "Knife Fight",
            LRType.NoScope => "No Scope",
            LRType.FiftyFifty => "Fifty Fifty",
            LRType.ShotForShot => "Shot For Shot",
            LRType.MagForMag => "Mag For Mag",
            LRType.Race => "Race",
            _ => "Unknown"
        };
    }

    public static LRType? FromFriendlyString(string type)
    {
        foreach (var value in Enum.GetValues(typeof(LRType)))
        {
            if (ToFriendlyString((LRType)value).Equals(type))
                return (LRType)value;
            if (ToFriendlyString((LRType)value).ToLower().Replace(" ", "").Equals(type.ToLower().Replace(" ", "")))
                return (LRType)value;
            if (((LRType)value).ToString().ToLower().Replace(" ", "").Equals(type.ToLower().Replace(" ", "")))
                return (LRType)value;
        }

        return null;
    }

    public static LRType FromIndex(int index)
    {
        return (LRType)index;
    }

    public static LRType? FromString(string type)
    {
        if (Enum.TryParse<LRType>(type, true, out var result))
            return result;
        return null;
    }

}