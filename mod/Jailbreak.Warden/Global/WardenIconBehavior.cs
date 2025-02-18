using System.Drawing;
using CounterStrikeSharp.API.Core;
using Gangs.WardenIconPerk;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden.Global;

public class WardenIconBehavior(ITextSpawner spawner)
  : GenericIconBehavior(spawner, Color.Blue), IWardenIcon {
  public void AssignWardenIcon(CCSPlayerController warden) {
    AssignIcon(warden);
  }

  public void RemoveWardenIcon(CCSPlayerController warden) {
    RemoveIcon(warden);
  }

  override protected async Task<string> getIcon(PlayerWrapper player) {
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();

    if (playerStats == null) return WardenIcon.DEFAULT.GetIcon();

    var icon =
      await playerStats.GetForPlayer<WardenIcon>(player,
        WardenIconPerk.STAT_ID);

    if (gangStats == null || players == null) return icon.GetIcon();

    var gangPlayer = await players.GetPlayer(player.Steam);
    if (gangPlayer?.GangId == null) return WardenIcon.DEFAULT.GetIcon();
    var available =
      await gangStats.GetForGang<WardenIcon>(gangPlayer.GangId.Value,
        WardenIconPerk.STAT_ID);

    if ((available & icon) == 0) return WardenIcon.DEFAULT.GetIcon();
    return icon != WardenIcon.RANDOM ? icon.GetIcon() : available.PickRandom();
  }
}