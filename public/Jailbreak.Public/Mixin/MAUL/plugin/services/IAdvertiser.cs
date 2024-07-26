using CounterStrikeSharp.API.Modules.Commands;

namespace api.plugin.services;

public interface IAdvertiser
{
    void SendAdvertisement();
    void SendAllAdvertisements(CommandInfo info);
    void StartRepeat(float period);
}