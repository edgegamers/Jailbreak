using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jailbreak.Public.Mod.LastRequest.Enums;

public enum LRType
{
    GunToss,
    RockPaperScissors,
    KnifeFight,
    NoScope,
    Coinflip,
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
            LRType.Coinflip => "Coinflip",
            _ => "Unknown"
        };
    }

    public static LRType FromIndex(int index)
    {
        return (LRType)index;
    }

    public static LRType? FromString(string type)
    {
        if (Enum.TryParse<LRType>(type, true, out var result))
            return result;
        type = type.ToLower().Replace(" ", "");
        switch (type)
        {
            case "rps":
                return LRType.RockPaperScissors;
            case "s4s":
            case "sfs":
                return LRType.ShotForShot;
            case "m4m":
            case "mfm":
                return LRType.MagForMag;
        }

        if (type.Contains("knife"))
            return LRType.KnifeFight;
        if (type.Contains("scope"))
            return LRType.NoScope;
        if (type.Contains("gun"))
            return LRType.GunToss;
        if (type.Contains("coin") || type.Contains("fifty"))
            return LRType.Coinflip;
        return null;
    }
}