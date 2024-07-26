using api.plugin.services;
using CounterStrikeSharp.API.Core;
using maul.api.services;
using plugin;

namespace api.plugin;

/// <summary>
///     The Actain plugin interface. All services and configuration are accessed through this interface.
/// </summary>
public interface IActain : IPluginConfig<ActainConfig>
{
    IMaulService getMaulService();
    BasePlugin getBase();
    IAnnouncer getAdminActionsAnnouncer();
    IAdminChat getAdminChatAnnouncer();
    ILeadershipChat getLeadershipChatAnnouncer();
    IGagService getGagService();
    IChatColorsService getChatColorsService();
    ITagService getTagService();
    IAuthenticator getAuthenticator();
    ICookieService getCookieService();
    IAdvertiser getAdvertiser();
    ISanitizer getSanitizer();
    IRenamer getRenamer();
    IModeration getModerator();
}