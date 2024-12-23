using System.Diagnostics;
using Gangs.BaseImpl;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;

namespace WardenPaintColorPerk;

public class WardenPaintColorPerk(IServiceProvider provider)
  : BasePerk<WardenPaintColor>(provider) {
  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public const string STAT_ID = "jb_wardenpaintcolor";
  public const string DESC = "Change the color of your warden paint!";

  public override string StatId => STAT_ID;
  public override string Name => "Paint Color";
  public override string? Description => DESC;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var (success, data) =
      await gangStats.GetForGang<WardenPaintColor>(player.GangId.Value,
        STAT_ID);
    if (!success) data = WardenPaintColor.DEFAULT;
    var (_, equipped) =
      await playerStats.GetForPlayer<WardenPaintColor>(player.Steam, STAT_ID);
    return new WardenPaintColorMenu(Provider, data, equipped);
  }

  public override WardenPaintColor Value { get; set; }
}