using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Handles sending messages to the leadership chat.
/// </summary>
public interface ILeadershipChat
{
    /// <summary>
    ///     Sends a message to the leadership chat.
    /// </summary>
    /// <param name="sender">The user sending the message, null for console</param>
    /// <param name="target">The target for the message, if null then this is all leadership in general</param>
    /// <param name="message">The message to send</param>
    /// <param name="allChat">If true this message was sent in all chat, if false this was sent in team chat</param>
    void SendLeadershipChat(CCSPlayerController? sender, CCSPlayerController? target, string message, bool allChat);
}