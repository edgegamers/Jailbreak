using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using IMenu = GangsAPI.Services.Menu.IMenu;

namespace Gangs.SpecialIconPerk;

public class SpecialIconPerk(IServiceProvider provider)
  : BasePerk<SpecialIcon>(provider) {
  public const string STAT_ID = "jb_specialicon";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "ST Icon";

  public override string Description
    => "Change the icon that appears above your head as ST";

  public override SpecialIcon Value { get; set; } = SpecialIcon.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<SpecialIcon>(player.GangId.Value, STAT_ID);
    var equipped =
      await playerStats.GetForPlayer<SpecialIcon>(player.Steam, STAT_ID);
    return new SpecialIconMenu(Provider, data, equipped);
  }
}