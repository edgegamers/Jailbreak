using GangsAPI.Data.Gang;
using GangsAPI.Data.Stat;
using GangsAPI.Perks;
using GangsAPI.Services.Menu;

namespace Gangs.BaseImpl;

public abstract class BasePerk : BaseStat, IPerk {
  public abstract Task<int?> GetCost(IGangPlayer player);
  public abstract Task OnPurchase(IGangPlayer player);
  public abstract Task<IMenu?> GetMenu(IGangPlayer player);
}

public abstract class BasePerk<TV>(IServiceProvider provider)
  : BasePerk, IPerk, IStat<TV> {
  protected IServiceProvider Provider { get; } = provider;
  public override Type ValueType => typeof(TV);
  public abstract TV Value { get; set; }

  public bool Equals(IStat<TV>? other) {
    return other is not null && StatId == other.StatId;
  }
}

public abstract class BaseStat : IStat {
  public bool Equals(IStat? other) {
    return other is not null && StatId == other.StatId;
  }

  public abstract string StatId { get; }
  public abstract string Name { get; }
  public abstract string? Description { get; }
  public abstract Type ValueType { get; }
}

public abstract class BaseStat<V> : BaseStat, IStat<V?> {
  public override Type ValueType => typeof(V);

  public bool Equals(IStat<V?>? other) {
    return other is not null && StatId == other.StatId;
  }

  public abstract V? Value { get; set; }
}