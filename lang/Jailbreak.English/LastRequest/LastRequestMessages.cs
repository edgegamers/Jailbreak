using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.English.LastRequest;

public class LastRequestMessages : ILastRequestMessages, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.DarkRed}[{ChatColors.LightRed}LR{ChatColors.DarkRed}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView LastRequestEnabled()
    {
        return new SimpleView
        {
            {
                PREFIX,
                $"Last Request has been enabled. {ChatColors.Grey}Type {ChatColors.LightBlue}!lr{ChatColors.Grey} to start a last request."
            }
        };
    }

    public IView LastRequestDisabled()
    {
        return new SimpleView
        {
            { PREFIX, $"{ChatColors.Grey}Last Request has been {ChatColors.Red}disabled{ChatColors.Grey}." }
        };
    }

    public IView LastRequestNotEnabled()
    {
        return new SimpleView
        {
            { PREFIX, $"{ChatColors.Red}Last Request is not enabled." }
        };
    }

    public IView InvalidLastRequest(string query)
    {
        return new SimpleView
        {
            PREFIX,
            "Invalid Last Request: ",
            query
        };
    }

    public IView InvalidPlayerChoice(CCSPlayerController player, string reason)
    {
        return new SimpleView
        {
            PREFIX,
            "Invalid player choice: ",
            player,
            " Reason: ",
            reason
        };
    }

    public IView InformLastRequest(AbstractLastRequest lr)
    {
        return new SimpleView
        {
            PREFIX,
            lr.prisoner, "is preparing a", lr.type.ToFriendlyString(),
            "Last Request against", lr.guard
        };
    }

    public IView AnnounceLastRequest(AbstractLastRequest lr)
    {
        return new SimpleView
        {
            PREFIX,
            lr.prisoner, "is doing a", lr.type.ToFriendlyString(),
            "Last Request against", lr.guard
        };
    }

    public IView LastRequestDecided(AbstractLastRequest lr, LRResult result)
    {
        return new SimpleView
        {
            PREFIX,
            (result == LRResult.GuardWin ? ChatColors.Blue : ChatColors.Red).ToString(),
            result == LRResult.PrisonerWin ? lr.prisoner : lr.guard,
            "won the LR."
        };
    }

    public IView DamageBlockedInsideLastRequest => new SimpleView { PREFIX, "You or they are in LR, damage blocked." };

    public IView DamageBlockedNotInSameLR => new SimpleView
        { PREFIX, "You are not in the same LR as them, damage blocked." };
}