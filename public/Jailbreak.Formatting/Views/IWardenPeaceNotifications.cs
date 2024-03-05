using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IWardenPeaceNotifications
{
    // message for when css_peace command is run to notify Prisoners/Guards that the warden has silenced everybody
    public IView PLAYERS_MUTED_VIACMD { get; }

    // self explanatory if you read above
    public IView PLAYERS_UNMUTED_VIACMD { get; }

    // message when prisoners have been muted automatically when the round starts
    public IView PRISONERS_MUTED_STARTROUND { get; }

    public IView PRISONERS_UNMUTED_STARTROUND { get; }

    public IView PLAYERS_MUTED_FIRSTWARDEN { get; }

    public IView PLAYERS_UNMUTED_FIRSTWARDEN { get; }

    public IView PLAYERS_WARDEN_DIED { get; }

    public IView PLAYERS_UNMUTED_ADMINCMD { get; }

    public IView PLAYERS_UNMUTED_ROUNDEND { get; }

    public IView CSS_PEACE_COOLDOWN(float cooldownTime);

}
