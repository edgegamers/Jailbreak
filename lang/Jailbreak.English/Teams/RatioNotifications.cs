using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Teams;

public class RatioNotifications : IRatioNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject Prefix =
        new HiddenFormatObject($" {ChatColors.LightRed}[{ChatColors.Red}JB{ChatColors.LightRed}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView NotEnoughGuards => new SimpleView(writer =>
        writer
            .Line(Prefix, "There's not enough guards in the queue!"));

    public IView PleaseJoinGuardQueue => new SimpleView(writer =>
        writer
            .Line(Prefix, "Type !guard to become a guard!"));

    public IView JoinedGuardQueue => new SimpleView(writer =>
        writer
            .Line(Prefix, "You've joined the guard queue!"));

    public IView AlreadyAGuard => new SimpleView(writer =>
        writer
            .Line(Prefix, "You're already a guard!"));

    public IView YouWereAutobalancedPrisoner => new SimpleView(writer =>
        writer
            .Line(Prefix, "You were autobalanced to the prisoner team!"));

    public IView AttemptToJoinFromTeamMenu => new SimpleView(writer =>
        writer
            .Line(Prefix, "You were swapped back to the prisoner team!")
            .Line(Prefix, "Please use !guard to join the guard team."));

    public IView LeftGuard => new SimpleView(writer =>
        writer
            .Line(Prefix, "You are no longer a guard.")
            .Line(Prefix, "Please use !guard if you want to re-join the guard team."));

    public IView YouWereAutobalancedGuard => new SimpleView(writer =>
        writer
            .Line(Prefix, "You are now a guard!"));
}