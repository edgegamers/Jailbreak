using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombPerk(IServiceProvider provider)
  : BasePerk<BombPerkData>(provider) {
  public const string STAT_ID = "jb_bomb_icon";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Bomb Icon";

  public override string Description
    => "Customize the icon that is shown when you bomb a CT";

  public override BombPerkData Value { get; set; } = new();

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    throw new NotImplementedException();
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");

    var (success, data) =
      await gangStats.GetForGang<BombPerkData>(player.GangId.Value, STAT_ID);

    if (!success || data == null) data = new BombPerkData();

    return new BombIconMenu(Provider, data);
  }
}

public class BombPerkData {
  public BombIcon Unlocked { get; set; }
  public BombIcon Equipped { get; set; }
}