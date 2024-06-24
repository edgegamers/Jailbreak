using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.LastGuard;

public class LastGuardMessages : ILastGuardMessages
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.DarkRed}[{ChatColors.LightRed}JB{ChatColors.DarkRed}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView LastGuardActivated(CCSPlayerController guard, int prisonerHp) => new SimpleView()
    {
        {
            PREFIX, "Last Guard has been activated.", guard, "has", guard.Health, "HP vs. the prisoners'", prisonerHp,
            "."
        }
    };

    public IView LastGuardOver(CCSPlayerController? winner) => new SimpleView()
    {
        winner == null
            ? [PREFIX, "Last Guard has ended."]
            : [PREFIX, winner, "has won Last Guard."]
    };
}