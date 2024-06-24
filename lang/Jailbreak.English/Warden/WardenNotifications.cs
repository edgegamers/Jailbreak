﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class WardenNotifications : IWardenNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.Green}[{ChatColors.Olive}WARDEN{ChatColors.Green}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView PICKING_SHORTLY =>
        new SimpleView
        {
            { PREFIX, "Picking a warden shortly..." }, SimpleView.NEWLINE,
            { PREFIX, $"To enter the warden queue, type {ChatColors.Blue}!warden{ChatColors.White} in chat." }
        };

    public IView NO_WARDENS =>
        new SimpleView
        {
            PREFIX,
            $"No wardens in queue! The next player to run {ChatColors.Blue}!warden{ChatColors.White} will become a warden."
        };

    public IView WARDEN_LEFT =>
        new SimpleView { PREFIX, "The warden has left the game!" };

    public IView WARDEN_DIED =>
        new SimpleView { PREFIX, "The warden has died!" };

    public IView BECOME_NEXT_WARDEN =>
        new SimpleView { PREFIX, $"Type {ChatColors.Blue}!warden{ChatColors.White} to become the next warden" };

    public IView JOIN_RAFFLE =>
        new SimpleView { PREFIX, "You've joined the warden raffle." };

    public IView LEAVE_RAFFLE =>
        new SimpleView { PREFIX, "You've left the warden raffle." };

    public IView NOT_WARDEN =>
        new SimpleView { PREFIX, $"{ChatColors.LightRed}You are not the warden." };

    public IView FIRE_COMMAND_FAILED =>
		new SimpleView { PREFIX, "The fire command has failed to work for some unknown reason..." };

    public IView PASS_WARDEN(CCSPlayerController player)
    {
        return new SimpleView { PREFIX, player, "has resigned from being warden." };
    }

    public IView NEW_WARDEN(CCSPlayerController player)
    {
        return new SimpleView { PREFIX, player, "is now the warden!" };
    }

    public IView CURRENT_WARDEN(CCSPlayerController? player)
    {
        if (player is not null)
            return new SimpleView { PREFIX, "The warden is", player, "." };
        else
            return new SimpleView { PREFIX, "There is no warden." };
    }

    public IView FIRE_COMMAND_SUCCESS(CCSPlayerController player)
    {
        return new SimpleView { PREFIX, player, "has been fired and is no longer the warden." };
    }

}