using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Responsible for managing colors in chat.
/// </summary>
public interface ITagService
{
    /// <summary>
    ///     Gets the chat tag of a player.
    /// </summary>
    /// <param name="sender">Player to get chat color of</param>
    /// <returns>The color the player has selected for chat, the players team color if unset</returns>
    string GetTag(CCSPlayerController sender);

    /// <summary>
    ///     Gets the chat tag color of a player.
    /// </summary>
    /// <param name="sender">Player to get name color of</param>
    /// <returns>The color the player has selected for their name, ChatColors.White if unset</returns>
    char GetTagColor(CCSPlayerController sender);

    /// <summary>
    ///     Sets the chat tag of a player.
    /// </summary>
    /// <param name="sender">Player to set chat tag of</param>
    /// <param name="color">Tag to set</param>
    /// <returns>True if the tag was set, false if the tag was invalid</returns>
    bool SetTag(CCSPlayerController sender, string tag);

    /// <summary>
    ///     Sets the tag color color of a player.
    /// </summary>
    /// <param name="sender">Player to set chat tag color of</param>
    /// <param name="color">Color to set</param>
    /// <returns>True if the color was set, false if the color was invalid</returns>
    bool SetTagColor(CCSPlayerController sender, char color);

    /// <summary>
    ///     Gets the valid tags for a player.
    /// </summary>
    /// <param name="player">Player to get valid tags for</param>
    /// <returns>A list of valid tags for the player from DS Silver to the player's rank</returns>
    List<string> GetValidTags(CCSPlayerController player, bool includeLower = true);

    /// <summary>
    ///     Gets the valid tags for a rank tier.
    /// </summary>
    /// <param name="rank">Rank to get valid tags for</param>
    /// <returns>A list of valid tags for the player from DS Silver to the rank</returns>
    List<string> GetValidTags(int rank, bool includeLower = true);

    /// <summary>
    ///     Gets the rank for a tag.
    /// </summary>
    /// <param name="tag">Tag to get rank for</param>
    /// <returns>The tier of the rank needed to use the tag, -1 if invalid tag</returns>
    int RankForTag(string tag);
}