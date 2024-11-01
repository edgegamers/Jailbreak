namespace Gangs.BaseImpl.Stats;

public class WardenStat : BaseStat<WardenData> {
  public const string STAT_ID = "jb_warden_stat";
  public override string StatId => STAT_ID;
  public override string Name => "Guard";
  public override string? Description => "Stats revolving CT specific info";
  public override WardenData? Value { get; set; }
}

public class WardenData {
  public int TimesWardened { get; set; }
  public int WardenDeaths { get; set; }
  public int WardensKilled { get; set; }
  public int GuardDeathsAsWarden { get; set; }
  public int WardenDeathsAsGuard { get; set; }
}