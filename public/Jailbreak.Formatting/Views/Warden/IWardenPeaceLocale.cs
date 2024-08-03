using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenPeaceLocale {
  public IView UnmutedGuards { get; }

  public IView UnmutedPrisoners { get; }

  public IView MuteReminder { get; }

  public IView PeaceReminder { get; }

  public IView DeadReminder { get; }

  public IView AdminDeadReminder { get; }

  public IView PeaceActive { get; }
  public IView PeaceEnactedByAdmin(int seconds);

  public IView WardenEnactedPeace(int seconds);

  public IView GeneralPeaceEnacted(int seconds);
}