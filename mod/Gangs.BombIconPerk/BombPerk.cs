using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Menu;

namespace Gangs.BombIconPerk;

public class BombPerk(IServiceProvider provider)
  : BasePerk<BombPerkData>(provider) {
  public const string STAT_ID = "jb_bomb_icon";
  public override string StatId => STAT_ID;
  public override string Name => "Bomb Icon";

  public override string? Description
    => "Customize the icon that is shown when you bomb a CT";

  public override BombPerkData Value { get; set; } = new();

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    throw new NotImplementedException();
  }

  public override Task<IMenu?> GetMenu(IGangPlayer player) {
    return Task.FromResult<IMenu?>(new BombIconMenu());
  }
}

public class BombPerkData {
  public BombIcon Unlocked { get; set; }
  public BombIcon Equipped { get; set; }
}