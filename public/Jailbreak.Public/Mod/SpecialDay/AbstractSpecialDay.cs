using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.Public.Mod.SpecialDay;

public abstract class AbstractSpecialDay {
  protected BasePlugin Plugin;
  public abstract SDType Type { get; }
  public abstract ISpecialDayMessages Messages { get; }

  public AbstractSpecialDay(BasePlugin plugin) {
    Plugin = plugin;
    plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);
  }

  /// <summary>
  /// Called when the warden initially picks the special day.
  /// Use for teleporting, stripping weapons, starting timers, etc.
  /// </summary>
  public abstract void Setup();

  /// <summary>
  /// Called when the actual action begins for the special day.
  /// </summary>
  public abstract void Execute();

  [GameEventHandler]
  public abstract HookResult OnEnd(EventRoundEnd @event, GameEventInfo info);
}