﻿using System.Runtime.CompilerServices;
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

namespace Gangs.BaseImpl;

public abstract class AbstractEnumCommand<T>(IServiceProvider provider,
  string statId, T def, string title) : ICommand where T : struct, Enum {
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

  protected readonly IMenuManager Menus =
    provider.GetRequiredService<IMenuManager>();

  private readonly IPlayerManager players =
    provider.GetRequiredService<IPlayerManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  protected readonly IServiceProvider Provider = provider;

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  public abstract string Name { get; }

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

    var data = await gangStats.GetForGang<T>(player.GangId.Value, statId);
    var tmp  = await playerStats.GetForPlayer<T>(player.Steam, statId);

    if (info.Args.Length == 1) {
      openMenu(executor, data, tmp);
      return CommandResult.SUCCESS;
    }

    T   val;
    var query = string.Join('_', info.Args.Skip(1)).ToUpper();

    if (!int.TryParse(info[1], out var enumInt) || enumInt < 0) {
      if (!Enum.TryParse(query, out val)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "an icon"));
        return CommandResult.SUCCESS;
      }
    } else { val = (T)(object)enumInt; }

    if (!data.HasFlag(val)) {
      var (canPurchase, minRank) =
        await ranks.CheckRank(player, Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = getCost(val);

      if (await eco.TryPurchase(executor, cost,
        item: $"{title} ({formatItem(val)})") < 0)
        return CommandResult.SUCCESS;

      Unsafe.As<T, short>(ref data) |= Unsafe.As<T, short>(ref val);
      await gangStats.SetForGang(gang, statId, data);

      if (gangChat != null)
        await gangChat.SendGangChat(player, gang,
          localizer.Get(MSG.PERK_PURCHASED,
            player.Name ?? player.Steam.ToString(),
            $"{title} ({formatItem(val)})"));
      return CommandResult.SUCCESS;
    }

    await playerStats.SetForPlayer(executor, statId, val);
    executor.PrintToChat(
      $"{localizer.Get(MSG.PREFIX)}Set your {ChatColors.BlueGrey}{title} to {ChatColors.LightBlue}{formatItem(val)}.");
    return CommandResult.SUCCESS;
  }

  abstract protected void openMenu(PlayerWrapper player, T data, T equipped);

  abstract protected int getCost(T item);

  virtual protected string formatItem(T item) {
    return $"{item.ToString().ToTitleCase()}";
  }
}