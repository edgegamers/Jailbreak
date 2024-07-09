using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IJihadC4Notifications
{
    // public IView PlayerDetonateC4(CCSPlayerController player);
    // public IView JIHAD_C4_DROPPED { get; }
    public IView JIHAD_C4_PICKUP { get; }
    public IView JIHAD_C4_RECEIVED { get; }

    public IView JIHAD_C4_USAGE1 { get; }
    // public IView JIHAD_C4_USAGE2 { get; }
}