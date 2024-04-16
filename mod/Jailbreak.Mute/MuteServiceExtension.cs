using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Mute;

public static class MuteServiceExtension
{
    public static void AddJailbreakMute(this IServiceCollection services)
    {
        services.AddConfig<MuteConfig>("muteconfig");

        services.AddPluginBehavior<IMuteService, MuteSystem>();
    }
}