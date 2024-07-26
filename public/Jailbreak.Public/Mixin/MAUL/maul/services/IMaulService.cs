using maul.models;

namespace maul.api.services;

public interface IMaulService
{
    Task<UserInfo?> GetUserInfo(string identity, string ipAddress = "0.0.0.0");
    Task<UserInfo?> GetUserInfo(ulong identity, string ipAddress = "0.0.0.0");
    Task<BanInfo?> GetBanInfo(string identity);
    Task<BanRequest?> BanUser(string identity, string name, string bannerIdentity, long time, string reason);
}