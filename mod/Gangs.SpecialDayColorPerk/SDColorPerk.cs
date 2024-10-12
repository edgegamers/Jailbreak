using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Microsoft.Extensions.DependencyInjection;
using Stats.Perk.Smoke;

namespace Gangs.SpecialDayColorPerk;

public class SDColorPerk(IServiceProvider provider)
  : BasePerk<SDColorData>(provider) {
  public const string STAT_ID = "jb_sd_color";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Special Day Color";

  public override string? Description
    => "Change the color of your gang during special days!";

  public override SDColorData Value { get; set; } = new();

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var (success, data) =
      await gangStats.GetForGang<SDColorData>(player.GangId.Value, STAT_ID);
    if (!success || data == null) data = new SDColorData();
    return new SDColorMenu(Provider, data);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }
}

public class SDColorData {
  public SDColor Unlocked { get; set; }
  public SDColor Equipped { get; set; }
}