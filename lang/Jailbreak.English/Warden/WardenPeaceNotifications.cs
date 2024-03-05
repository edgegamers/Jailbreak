using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using static Jailbreak.English.Warden.WardenNotifications;
using static Jailbreak.English.Generic.GenericCommandNotifications;

namespace Jailbreak.English.Warden;

// todo add player name in notification and style it with colour
public class WardenPeaceNotifications : IWardenPeaceNotifications, ILanguage<Formatting.Languages.English>
{

    public IView PLAYERS_MUTED_VIACMD =>
        new SimpleView { WARDEN_PREFIX, "Prisoners and Guards are silenced for 10 seconds." };

    public IView PLAYERS_UNMUTED_VIACMD =>
        new SimpleView { WARDEN_PREFIX, "Prisoners and Guards can speak again."};

    public IView PRISONERS_MUTED_STARTROUND =>
        new SimpleView { GENERIC_PREFIX, "Prisoners are muted for 45 seconds."};

    public IView PRISONERS_UNMUTED_STARTROUND =>
    new SimpleView { GENERIC_PREFIX, "Prisoners can speak again." };

    public IView PLAYERS_MUTED_FIRSTWARDEN =>
        new SimpleView { GENERIC_PREFIX, "Prisoners and Guards are automatically silenced for 10 seconds."};

    public IView PLAYERS_UNMUTED_FIRSTWARDEN =>
        new SimpleView { GENERIC_PREFIX, "Prisoners and Guards can speak again." };

    public IView PLAYERS_WARDEN_DIED =>
        new SimpleView { GENERIC_PREFIX, "Warden is dead. Players can speak again." };

    public IView PLAYERS_UNMUTED_ADMINCMD =>
        new SimpleView { GENERIC_PREFIX, "An admin has removed the warden mute." };

    public IView PLAYERS_UNMUTED_ROUNDEND =>
        new SimpleView { GENERIC_PREFIX, "All players unmuted due to round end." };

    public IView CSS_PEACE_COOLDOWN(float cooldownTime) 
    {
        return new SimpleView { GENERIC_PREFIX, $"You\'re on cooldown for {Math.Ceiling(cooldownTime)} second{((Math.Ceiling(cooldownTime) == 1) ? "." : "s." )}" };
    }
        

}
