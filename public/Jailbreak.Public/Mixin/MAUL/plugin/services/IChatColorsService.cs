using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Responsible for managing colors in chat.
/// </summary>
public interface IChatColorsService
{
    /// <summary>
    ///     Gets the chat color of a player.
    /// </summary>
    /// <param name="sender">Player to get chat color of</param>
    /// <returns>The color the player has selected for chat, the players team color if unset</returns>
    char GetChatColor(CCSPlayerController sender);

    /// <summary>
    ///     Gets the name color of a player.
    /// </summary>
    /// <param name="sender">Player to get name color of</param>
    /// <returns>The color the player has selected for their name, ChatColors.White if unset</returns>
    char GetNameColor(CCSPlayerController sender);

    /// <summary>
    ///     Sets the chat color of a player.
    /// </summary>
    /// <param name="sender">Player to set chat color of</param>
    /// <param name="color">Color to set</param>
    /// <returns>True if the color was set, false if the color was invalid</returns>
    bool SetChatColor(CCSPlayerController sender, char color);

    /// <summary>
    ///     Sets the name color of a player.
    /// </summary>
    /// <param name="sender">Player to set name color of</param>
    /// <param name="color">Color to set</param>
    /// <returns>True if the color was set, false if the color was invalid</returns>
    bool SetNameColor(CCSPlayerController sender, char color);
}