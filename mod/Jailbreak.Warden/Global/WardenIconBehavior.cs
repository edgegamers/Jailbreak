using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Gangs.WardenIconPerk;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden.Global;

public class WardenIconBehavior(ITextSpawner spawner)
  : IPluginBehavior, IWardenIcon {
  private readonly IEnumerable<CPointWorldText>?[] wardenIcons =
    new IEnumerable<CPointWorldText>?[65];

  public void AssignWardenIcon(CCSPlayerController warden) {
    var wrapper = new PlayerWrapper(warden);

    Task.Run(async () => {
      var icon = await getIcon(wrapper);

      var data = new TextSetting { msg = icon, color = Color.Blue };

      await Server.NextFrameAsync(() => {
        var hat = spawner.CreateTextHat(data, warden);
        wardenIcons[warden.Slot] = hat;
      });
    });
  }

  public void RemoveWardenIcon(CCSPlayerController warden) {
    var hat = wardenIcons[warden.Slot];
    if (hat == null) return;
    foreach (var text in hat) {
      if (!text.IsValid) continue;
      text.Remove();
    }

    wardenIcons[warden.Slot] = null;
  }

  private async Task<string> getIcon(PlayerWrapper player) {
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();

    if (playerStats == null) return WardenIcon.DEFAULT.GetIcon();

    var (success, icon) =
      await playerStats.GetForPlayer<WardenIcon>(player,
        WardenIconPerk.STAT_ID);

    if (!success) icon = WardenIcon.DEFAULT;

    if (icon == WardenIcon.RANDOM && gangStats != null && players != null) {
      var gangPlayer = await players.GetPlayer(player.Steam);
      if (gangPlayer?.GangId == null) return WardenIcon.DEFAULT.GetIcon();
      var (_, available) =
        await gangStats.GetForGang<WardenIcon>(gangPlayer.GangId.Value,
          WardenIconPerk.STAT_ID);
      return available.PickRandom();
    }

    return icon.GetIcon();
  }
}