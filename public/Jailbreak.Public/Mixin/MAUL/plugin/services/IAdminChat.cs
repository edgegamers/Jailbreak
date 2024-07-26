using CounterStrikeSharp.API.Core;

namespace api.plugin.services;

/// <summary>
///     Handles sending messages to the admin chat.
/// </summary>
public interface IAdminChat
{
    /// <summary>
    ///     Sends a message to the admin chat.
    /// </summary>
    /// <param name="sender">The user sending the message, null for console</param>
    /// <param name="target">The target for the message, if null then this is all admins in general</param>
    /// <param name="message">The message to send</param>
    /// <param name="allChat">If true this message was sent in all chat, if false this was sent in team chat</param>
    void SendAdminChat(CCSPlayerController? sender, CCSPlayerController? target, string message, bool allChat);


    public void ResendAdminToPlayerMessage(CCSPlayerController? sender, string message);
}