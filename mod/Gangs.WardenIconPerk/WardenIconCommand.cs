using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Data.Command;
using GangsAPI.Exceptions;
using GangsAPI.Perks;
using GangsAPI.Permissions;
using GangsAPI.Services;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.WardenIconPerk;

public class WardenIconCommand(IServiceProvider provider) : ICommand {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

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

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  public string Name => "css_wardenicon";

  public string[] Aliases => [Name, "css_warden_icon"];

  public string[] Usage => ["<icon>"];

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

    var (success, data) = await gangStats.GetForGang<WardenIcon>(
      player.GangId.Value, WardenIconPerk.STAT_ID);

    if (!success) data = WardenIcon.DEFAULT;

    var (_, equipped) = await playerStats.GetForPlayer<WardenIcon>(
      player.Steam, WardenIconPerk.STAT_ID);

    if (info.Args.Length == 1) {
      var menu = new WardenIconMenu(provider, data, equipped);
      await menus.OpenMenu(executor, menu);
      return CommandResult.SUCCESS;
    }

    WardenIcon icon;
    var        query = string.Join('_', info.Args.Skip(1)).ToUpper();
    if (!int.TryParse(info[1], out var iconInt) || iconInt < 0) {
      if (!Enum.TryParse(query, out icon)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "an icon"));
        return CommandResult.SUCCESS;
      }
    } else
      icon = (WardenIcon)iconInt;

    if (!data.HasFlag(icon)) {
      var (canPurchase, minRank) = await ranks.CheckRank(player,
        Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = icon.GetCost();
      if (await eco.TryPurchase(executor, cost,
        item: "Warden Icon: " + icon.ToString().ToTitleCase()) < 0)
        return CommandResult.SUCCESS;

      data |= icon;
      await gangStats.SetForGang(gang, WardenIconPerk.STAT_ID, data);

      if (gangChat != null)
        await gangChat.SendGangChat(player, gang,
          localizer.Get(MSG.PERK_PURCHASED,
            $"Warden Icon ({icon.ToString().ToTitleCase()})"));
      return CommandResult.SUCCESS;
    }

    await playerStats.SetForPlayer(executor, WardenIconPerk.STAT_ID, icon);
    executor.PrintToChat(
      $"{localizer.Get(MSG.PREFIX)}Set your warden icon to {icon.GetIcon()} ({icon.ToString().ToTitleCase()}){ChatColors.Grey}.");
    return CommandResult.SUCCESS;
  }
}