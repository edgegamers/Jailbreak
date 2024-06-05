using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IGenericCommandNotifications
{
    public IView PlayerNotFound(string query);
    public IView PlayerFoundMultiple(string query);
    public IView CommandOnCooldown(DateTime cooldownEndsAt);
}