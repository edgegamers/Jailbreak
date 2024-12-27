using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.LastRequestColorPerk;

public class LRColorPerk(IServiceProvider provider)
  : BasePerk<LRColor>(provider) {
  public const string STAT_ID = "jb_lr_color";

  public const string DESC =
    "Pick the color of you and your partner during your LRs\\nConflicting colors are resolved by gang rank";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "LR Colors";

  public override string Description => DESC;

  public override LRColor Value { get; set; } = LRColor.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var (success, data) =
      await gangStats.GetForGang<LRColor>(player.GangId.Value, STAT_ID);
    if (!success) data = LRColor.DEFAULT;
    var (_, equipped) =
      await playerStats.GetForPlayer<LRColor>(player.Steam, STAT_ID);
    return new LRColorMenu(Provider, data, equipped);
  }
}