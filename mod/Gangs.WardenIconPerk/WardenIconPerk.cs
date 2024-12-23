using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using IMenu = GangsAPI.Services.Menu.IMenu;

namespace Gangs.WardenIconPerk;

public class WardenIconPerk(IServiceProvider provider)
  : BasePerk<WardenIcon>(provider) {
  public const string STAT_ID = "jb_wardenicon";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Warden Icon";

  public override string? Description
    => "Change the icon that appears above your head as warden";

  public override WardenIcon Value { get; set; } = WardenIcon.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var (success, data) =
      await gangStats.GetForGang<WardenIcon>(player.GangId.Value, STAT_ID);
    if (!success) data = WardenIcon.DEFAULT;
    var (_, equipped) =
      await playerStats.GetForPlayer<WardenIcon>(player.Steam, STAT_ID);
    return new WardenIconMenu(Provider, data, equipped);
  }
}