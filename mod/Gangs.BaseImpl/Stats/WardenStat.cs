using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Primitives;

namespace Gangs.BaseImpl.Stats;

public class WardenStat : BaseStat<WardenData> {
  public const string STAT_ID = "jb_warden_stat";
  public override string StatId => STAT_ID;
  public override string Name => "Guard";
  public override string Description => "Stats revolving CT specific info";
  public override WardenData? Value { get; set; }
}

public class WardenData {
  public int TimesWardened { get; set; }
  public int WardenDeaths { get; set; }
  public int WardensKilled { get; set; }
  public int GuardDeathsAsWarden { get; set; }
  public int WardenDeathsAsGuard { get; set; }

  public override string ToString() {
    var result =
      $"{ChatColors.BlueGrey}Times Wardened: {ChatColors.Yellow}{TimesWardened}\n";

    result +=
      $"{ChatColors.BlueGrey}Warden {ChatColors.LightBlue}Deaths{ChatColors.BlueGrey}: {ChatColors.Yellow}{WardenDeaths}\n";
    result +=
      $"{ChatColors.BlueGrey}Wardens {ChatColors.LightRed}Killed{ChatColors.BlueGrey}: {ChatColors.Yellow}{WardensKilled}\n";
    result +=
      $"{ChatColors.BlueGrey}Guard {ChatColors.LightRed}Deaths{ChatColors.BlueGrey} as Warden: {ChatColors.Yellow}{GuardDeathsAsWarden}\n";
    result +=
      $"{ChatColors.BlueGrey}Warden {ChatColors.LightRed}Deaths{ChatColors.BlueGrey} as Guard: {ChatColors.Yellow}{WardenDeathsAsGuard}";
    return result;
  }
}