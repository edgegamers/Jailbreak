using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Extensions;
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

namespace Gangs.LastRequestColorPerk;

public class LRColorCommand(IServiceProvider provider) : ICommand {
  public string Name => "css_lrcolor";
  public string[] Aliases => [Name, "css_lr_color"];

  public string[] Usage => ["<color>"];

  private readonly IPlayerManager players =
    provider.GetRequiredService<IPlayerManager>();

  private readonly IGangManager gangs =
    provider.GetRequiredService<IGangManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IStringLocalizer localizer =
    provider.GetRequiredService<IStringLocalizer>();

  private readonly IMenuManager menus =
    provider.GetRequiredService<IMenuManager>();

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  private readonly IGangChatPerk? gangChat =
    provider.GetService<IGangChatPerk>();

  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  private readonly IEcoManager eco = provider.GetRequiredService<IEcoManager>();

  public void Start() { commands.RegisterCommand(this); }

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
      await gangStats.GetForGang<LRColor>(gang, LRColorPerk.STAT_ID);

    if (!success) data = LRColor.DEFAULT;

    var (_, equipped) =
      await playerStats.GetForPlayer<LRColor>(player.Steam,
        LRColorPerk.STAT_ID);

    if (info.Args.Length == 1) {
      var menu = new LRColorMenu(provider, data, equipped);
      await menus.OpenMenu(executor, menu);
      return CommandResult.SUCCESS;
    }

    LRColor color;
    var     query = string.Join('_', info.Args.Skip(1)).ToUpper();
    if (!int.TryParse(info[1], out var colorInt) || colorInt < 0) {
      if (!Enum.TryParse(query, out color)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "a color"));
        return CommandResult.SUCCESS;
      }
    } else { color = (LRColor)colorInt; }

    if (!data.HasFlag(color)) {
      var (canPurchase, minRank) = await ranks.CheckRank(player,
        Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = color.GetCost();
      if (await eco.TryPurchase(executor, cost,
        item: "LR Color: " + color.ToString().ToTitleCase()) < 0)
        return CommandResult.SUCCESS;

      data |= color;
      await gangStats.SetForGang(gang, LRColorPerk.STAT_ID, data);

      if (gangChat != null)
        await gangChat.SendGangChat(player, gang,
          localizer.Get(MSG.PERK_PURCHASED, $"LR Color ({color})"));
      return CommandResult.SUCCESS;
    }

    await playerStats.SetForPlayer(executor, LRColorPerk.STAT_ID, color);
    executor.PrintToChat(localizer.Get(MSG.PREFIX) + "Set your LR color to "
      + color.GetColor().GetChatColor() + color.ToString().ToTitleCase()
      + ChatColors.Grey + ".");

    return CommandResult.SUCCESS;
  }
}