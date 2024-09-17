using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Data.Command;
using GangsAPI.Exceptions;
using GangsAPI.Extensions;
using GangsAPI.Perks;
using GangsAPI.Permissions;
using GangsAPI.Services;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.BombIconPerk;

public class BombIconCommand(IServiceProvider provider) : ICommand {
  private readonly IEcoManager eco = provider.GetRequiredService<IEcoManager>();

  private readonly IGangChatPerk? gangChat =
    provider.GetService<IGangChatPerk>();

  private readonly IGangManager gangs =
    provider.GetRequiredService<IGangManager>();

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IStringLocalizer localizer =
    provider.GetRequiredService<IStringLocalizer>();

  private readonly IMenuManager menus =
    provider.GetRequiredService<IMenuManager>();

  private readonly IPlayerManager players =
    provider.GetRequiredService<IPlayerManager>();

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  public string Name => "css_bombicon";
  public string[] Usage => ["<icon>"];

  public void Start() {
    provider.GetRequiredService<ICommandManager>().RegisterCommand(this);
  }

  public async Task<CommandResult> Execute(PlayerWrapper? executor,
    CommandInfoWrapper info) {
    if (executor == null) return CommandResult.PLAYER_ONLY;
    var player = await players.GetPlayer(executor.Steam)
      ?? throw new PlayerNotFoundException(executor.Steam);
    if (player.GangId == null) {
      info.ReplySync(localizer.Get(MSG.NOT_IN_GANG));
      return CommandResult.SUCCESS;
    }

    var gang = await gangs.GetGang(player.GangId.Value)
      ?? throw new GangNotFoundException(player.GangId.Value);

    var (success, data) =
      await gangStats.GetForGang<BombPerkData>(gang, BombPerk.STAT_ID);

    if (!success || data == null) data = new BombPerkData();

    if (info.ArgCount == 1) {
      var menu = new BombIconMenu();
      await menus.OpenMenu(executor, menu);
      return CommandResult.SUCCESS;
    }

    BombIcon icon;
    if (!int.TryParse(info[1], out var iconInt) || iconInt < 0) {
      if (!Enum.TryParse(info[1], out icon)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "an icon"));
        return CommandResult.SUCCESS;
      }
    } else { icon = (BombIcon)iconInt; }

    if (!data.Unlocked.HasFlag(icon)) {
      var (canPurchase, minRank) = await ranks.CheckRank(player,
        Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = icon.GetCost();
      if (await eco.TryPurchase(executor, cost,
        item: "Bomb Icon: " + icon.ToString().ToTitleCase()) < 0)
        return CommandResult.SUCCESS;

      data.Unlocked |= icon;

      await gangStats.SetForGang(gang, BombPerk.STAT_ID, data);

      if (gangChat == null) return CommandResult.SUCCESS;

      await gangChat.SendGangChat(player, gang,
        localizer.Get(MSG.PERK_PURCHASED, icon.ToString()));
      return CommandResult.SUCCESS;
    }


    var (canManage, required) =
      await ranks.CheckRank(player, Perm.MANAGE_PERKS);
    if (!canManage) {
      info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, required.Name));
      return CommandResult.SUCCESS;
    }

    data.Equipped = icon;
    await gangStats.SetForGang(gang, BombPerk.STAT_ID, data);

    if (gangChat == null) return CommandResult.SUCCESS;

    await gangChat.SendGangChat(player, gang,
      localizer.Get(MSG.GANG_THING_SET, "Bomb Icon",
        icon.ToString().ToTitleCase()));
    return CommandResult.SUCCESS;
  }
}