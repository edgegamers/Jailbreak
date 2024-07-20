using Jailbreak.Formatting.Base;
using Jailbreak.Public.Mod.Mute;

namespace Jailbreak.Formatting.Views;

public interface IPeaceMessages {
  public IView UnmutedGuards { get; }

  public IView UnmutedPrisoners { get; }

  public IView MuteReminder { get; }

  public IView PeaceReminder { get; }

  public IView DeadReminder { get; }

  public IView AdminDeadReminder { get; }

  public IView PeaceActive { get; }
  public IView PeaceEnacted(int seconds, MuteReason reason);

}