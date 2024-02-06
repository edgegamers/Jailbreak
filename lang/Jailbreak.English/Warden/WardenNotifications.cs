using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class WardenNotifications : IWardenNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject Prefix =
        new HiddenFormatObject($" {ChatColors.Lime}[{ChatColors.Green}WARDEN{ChatColors.Lime}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView PickingShortly => new SimpleView(writer =>
        writer
            .Line(Prefix, "Picking a warden shortly")
            .Line(Prefix, "To enter the warden queue, type !warden in chat."));

    public IView NoWardens => new SimpleView(writer =>
        writer
            .Line(Prefix, "No wardens in queue! The next player to run !warden will become a warden."));

    public IView WardenLeft => new SimpleView(writer =>
        writer.Line(Prefix, "The warden has left the game!"));

    public IView WardenDied => new SimpleView(writer =>
        writer.Line(Prefix, "The warden has died!"));

    public IView BecomeNextWarden => new SimpleView(writer =>
        writer.Line(Prefix, "Type !warden to become the next warden"));

    public IView JoinRaffle => new SimpleView(writer =>
        writer.Line(Prefix, "You've joined the warden raffle!"));

    public IView LeaveRaffle => new SimpleView(writer =>
        writer.Line(Prefix, "You've left the warden raffle!"));

    public IView PassWarden(CCSPlayerController player)
    {
        return new SimpleView(writer =>
            writer.Line(Prefix, player, "has resigned from being warden!"));
    }

    public IView NewWarden(CCSPlayerController player)
    {
        return new SimpleView(writer =>
            writer.Line(Prefix, player, "is now the warden!"));
    }

    public IView CurrentWarden(CCSPlayerController? player)
    {
        if (player is not null)
            return new SimpleView(writer =>
                writer.Line(Prefix, "The current warden is", player));
        return new SimpleView(writer =>
            writer.Line(Prefix, "There is currently no warden!"));
    }
}