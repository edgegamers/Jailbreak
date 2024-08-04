using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISDInstanceLocale : ISDLocale {
  public string Name { get; }
  public string[] Description { get; }

  public IView SpecialDayStart { get; }

  public IView SpecialDayEnd { get; }

  public IView BeginsIn(int seconds);

  public IView RunnerAssigned(CCSPlayerController player);

  public IView YouAreRunner(int seconds);

  public IView RunnerReassigned(CCSPlayerController player);

  public IView RuntimeLeft(int seconds);

  public IView BeginRound(int round, int eliminations, int seconds);

  public IView BestTime(CCSPlayerController player, float time);

  public IView PlayerTime(CCSPlayerController player, int place, float time);

  public IView ImpossibleLocation(CsTeam team, CCSPlayerController player);

  public IView PlayerWon(CCSPlayerController player);

  public IView NoneEliminated { get; }

  public IView PlayerEliminated(CCSPlayerController player);
}