using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

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
}