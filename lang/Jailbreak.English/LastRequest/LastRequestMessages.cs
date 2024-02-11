using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.LastRequest;

public class LastRequestMessages : ILastRequestMessages
{
    public IView LastRequestEnabled() => new SimpleView()
    {
        { "Last Request has been enabled." }
    };

    public IView LastRequestDisabled() => new SimpleView()
    {
        { "Last Request has been disabled." }
    };
}