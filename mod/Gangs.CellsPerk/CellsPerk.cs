using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.CellsPerk;

public class CellsPerk(IServiceProvider provider) : BasePerk<int>(provider) {
  public const string STAT_ID = "jb_cells";
  public override string StatId => STAT_ID;
  public override string Name => "Hide in Cells";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  public override string? Description
    => "# of gang members that can hide in cells without being detected";

  public override async Task<int?> GetCost(IGangPlayer player) {
    if (player.GangId == null || player.GangRank == null) return null;
    var (success, cells) =
      await gangStats.GetForGang<int>(player.GangId.Value, StatId);

    if (!success) cells = 0;

    return getCostFor(cells + 1);
  }

  // https://www.desmos.com/calculator/ie4owyajay
  private static int getCostFor(int size) {
    var numerator = 100 * size + 4.9 * Math.Pow(size, 5);
    return (int)(Math.Ceiling(numerator / 500) * 100);
  }

  public override async Task OnPurchase(IGangPlayer player) {
    if (player.GangId == null || player.GangRank == null) return;
    var (success, cells) =
      await gangStats.GetForGang<int>(player.GangId.Value, StatId);
    if (!success) cells = 1;
    cells++;
    await gangStats.SetForGang(player.GangId.Value, StatId, cells);
  }

  public override Task<IMenu?> GetMenu(IGangPlayer player) {
    var menu = new BasicPerkMenu(Provider, this);
    return Task.FromResult<IMenu?>(menu);
  }

  public override int Value { get; set; }
}