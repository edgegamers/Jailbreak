using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.LastRequest;

public class RaceLRMessages : IRaceLRMessages, ILanguage<Formatting.Languages.English>
{
    public IView END_RACE_INSTRUCTION => new SimpleView()
    {
        { LastRequestMessages.PREFIX, "Type !endrace to set the end point!"}, SimpleView.NEWLINE,
        { LastRequestMessages.PREFIX, "Type !endrace to set the end point!"}, SimpleView.NEWLINE,
        { LastRequestMessages.PREFIX, "Type !endrace to set the end point!"}, SimpleView.NEWLINE,
    };

    public IView RACE_STARTING_MESSAGE(CCSPlayerController prisoner)
    {
        return new SimpleView()
        {
            {
                LastRequestMessages.PREFIX, prisoner,
                " is starting a race. Pay attention to where they set the end point!"
            }
        };
    }
}