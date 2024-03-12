using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IWardenLastGuardNotifications
{

    public IView LASTGUARD_ACTIVATED(string wardenDisplayName);

    public IView LASTGUARD_MAXHEALTH(int maxHealth);

    public IView LASTGUARD_TIMELIMIT(int timeInSeconds);

}
