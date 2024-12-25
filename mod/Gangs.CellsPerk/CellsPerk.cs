using Gangs.BaseImpl;
using GangsAPI;
using GangsAPI.Data.Gang;
using GangsAPI.Perks;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.CellsPerk;

public class CellsPerk(IServiceProvider provider) : BasePerk<int>(provider) {
  public const string STAT_ID = "jb_cells";

  private readonly IGangManager gangs =
    provider.GetRequiredService<IGangManager>();

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IStringLocalizer localizer =
    provider.GetRequiredService<IStringLocalizer>();

  public override string StatId => STAT_ID;
  public override string Name => "Hide in Cells";

  public override string Description
    => "# of gang members that can hide in cells without being detected";

  public override int Value { get; set; }

  public override async Task<int?> GetCost(IGangPlayer player) {
    if (player.GangId == null || player.GangRank == null) return null;
    var (success, cells) =
      await gangStats.GetForGang<int>(player.GangId.Value, StatId);

    if (!success) cells = 0;

    return getCostFor(cells + 1);
  }

  // https://www.desmos.com/calculator/u94hnq6cw0
  private static int getCostFor(int size) {
    var numerator = 40 * size + 5 * Math.Pow(size, 3);
    return (int)(Math.Ceiling(numerator / 10) * 100);
  }

  public override async Task OnPurchase(IGangPlayer player) {
    if (player.GangId == null || player.GangRank == null) return;
    var (success, cells) =
      await gangStats.GetForGang<int>(player.GangId.Value, StatId);
    if (!success) cells = 1;
    cells++;
    await gangStats.SetForGang(player.GangId.Value, StatId, cells);

    var gang     = await gangs.GetGang(player.GangId.Value);
    var gangChat = Provider.GetService<IGangChatPerk>();

    var str = localizer.Get(MSG.PERK_PURCHASED,
      player.Name ?? player.Steam.ToString(), $"{Name} ({cells})");
    if (gang != null && gangChat != null)
      await gangChat.SendGangChat(gang, str);
  }

  public override Task<IMenu?> GetMenu(IGangPlayer player) {
    var menu = new BasicPerkMenu(Provider, this);
    return Task.FromResult<IMenu?>(menu);
  }
}