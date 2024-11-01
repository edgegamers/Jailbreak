namespace Gangs.BaseImpl.Stats;

public class LGStat : BaseStat<LGData> {
  public const string STAT_ID = "jb_lg_stat";
  public override string StatId => STAT_ID;
  public override string Name => "LGs";
  public override string? Description => "LGs reached";
  public override LGData? Value { get; set; }
}

public class LGData {
  public int CtLgs { get; set; }
  public int TLgs { get; set; }

  public override string ToString() { return $"CT/T Last Guards: {CtLgs}/{TLgs}"; }
}