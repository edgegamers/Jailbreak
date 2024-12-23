using System.Drawing;
using CounterStrikeSharp.API.Core;
using Gangs.SpecialIconPerk;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden.Global;

public class SpecialIconBehavior(ITextSpawner spawner)
  : GenericIconBehavior(spawner, Color.Green), ISpecialIcon {
  public void AssignSpecialIcon(CCSPlayerController player) {
    AssignIcon(player);
  }

  public void RemoveSpecialIcon(CCSPlayerController player) {
    RemoveIcon(player);
  }

  override protected async Task<string> getIcon(PlayerWrapper player) {
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();

    if (playerStats == null) return SpecialIcon.DEFAULT.GetIcon();

    var (success, icon) =
      await playerStats.GetForPlayer<SpecialIcon>(player,
        SpecialIconPerk.STAT_ID);

    if (!success) icon = SpecialIcon.DEFAULT;

    if (gangStats == null || players == null) return icon.GetIcon();

    var gangPlayer = await players.GetPlayer(player.Steam);
    if (gangPlayer?.GangId == null) return SpecialIcon.DEFAULT.GetIcon();
    var (_, available) =
      await gangStats.GetForGang<SpecialIcon>(gangPlayer.GangId.Value,
        SpecialIconPerk.STAT_ID);

    if ((available & icon) == 0) return SpecialIcon.DEFAULT.GetIcon();
    return icon != SpecialIcon.RANDOM ? icon.GetIcon() : available.PickRandom();
  }
}