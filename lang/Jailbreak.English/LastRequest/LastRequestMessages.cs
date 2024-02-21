using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.English.LastRequest;

public class LastRequestMessages : ILastRequestMessages, ILanguage<Formatting.Languages.English>
{
    public IView LastRequestEnabled() => new SimpleView()
    {
        { "Last Request has been enabled." }
    };

    public IView LastRequestDisabled() => new SimpleView()
    {
        { "Last Request has been disabled." }
    };
    
    public IView LastRequestNotEnabled() => new SimpleView()
    {
        { "Last Request is not enabled." }
    };

    public IView InvalidLastRequest(string query)
    {
        return new SimpleView()
        {
            "Invalid Last Request: ",
            query
        };
    }

    public IView InvalidPlayerChoice(CCSPlayerController player, string reason)
    {
        return new SimpleView()
        {
            "Invalid player choice: ",
            player,
            " Reason: ",
            reason
        };
    }

    public IView InformLastRequest(AbstractLastRequest lr)
    {
        return new SimpleView()
        {
            lr.prisoner, "is", lr.type.ToFriendlyString(),
            "against", lr.guard
        };
    }
    
    public IView AnnounceLastRequest(AbstractLastRequest lr)
    {
        return new SimpleView()
        {
            lr.prisoner, "is", lr.type.ToFriendlyString(),
            "against", lr.guard
        };
    }

    public IView LastRequestDecided(AbstractLastRequest lr, LRResult result)
    {
        return new SimpleView()
        {
            lr.prisoner, "'s LR has been decided: ",
            result == LRResult.PrisonerWin ? lr.prisoner : lr.guard
        };
    }
}