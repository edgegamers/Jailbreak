using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IPeaceMessages
{
    public IView PEACE_ENACTED_BY_ADMIN(int seconds);

    public IView WARDEN_ENACTED_PEACE(int seconds);

    public IView GENERAL_PEACE_ENACTED(int seconds);

    public IView UNMUTED_GUARDS { get; }

    public IView UNMUTED_PRISONERS { get; }

    public IView MUTE_REMINDER { get; }
    
    public IView PEACE_REMINDER { get; }

    public IView DEAD_REMINDER { get; }
}