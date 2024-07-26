using api.plugin.models;

namespace api.plugin.services;

public interface ICookieService
{
    Task<ICookie?> FindClientCookie(string key);
    Task<ICookie> RegClientCookie(string key);
}