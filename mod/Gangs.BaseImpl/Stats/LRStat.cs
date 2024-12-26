namespace Gangs.BaseImpl.Stats;

public class LRStat : BaseStat<LRData> {
  public const string STAT_ID = "jb_lr_stat";
  public override string StatId => STAT_ID;
  public override string Name => "LRs";
  public override string Description => "LRs reached";
  public override LRData? Value { get; set; }
}

public class LRData {
  public int LRsReachedAsCt { get; set; }
  public int LRsReachedAsT { get; set; }
  public int CtLrs { get; set; }
  public int TLrs { get; set; }
  public int CTLrsWon { get; set; }
  public int TLrsWon { get; set; }

  public override string ToString() {
    return $"CT/T LRs, CT/T LR Wins: {CtLrs}/{TLrs} {CTLrsWon}/{TLrsWon}";
  }
}