using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IJihadC4Notifications
{

    public IView JihadBombDelaySet(float delay);

    public IView JIHAD_DELAY_CMD_USAGE { get; }

    public IView JihadCurrentDelay(float delay);

    public IView JIHAD_GIVEN_BOMB {  get; }


}
