using System.Runtime.CompilerServices;
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
  string statId, T def, string title, bool isOnlyGang)
  : ICommand where T : struct, Enum {
  abstract protected void openMenu(PlayerWrapper player, T data, T equipped);

  protected readonly IServiceProvider Provider = provider;

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

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  public abstract string Name { get; }

  abstract protected int getCost(T item);

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
      await gangStats.GetForGang<T>(player.GangId.Value, statId);

    if (!success) data = def;

    var (_, equipped) = await playerStats.GetForPlayer<T>(player.Steam, statId);

    if (info.Args.Length == 1) {
      openMenu(executor, data, equipped);
      return CommandResult.SUCCESS;
    }

    T   val;
    var query = string.Join('_', info.Args.Skip(1)).ToUpper();

    if (!int.TryParse(info[1], out var enumInt) || enumInt < 0) {
      if (!Enum.TryParse<T>(query, out val)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "an icon"));
        return CommandResult.SUCCESS;
      }
    } else
      val = (T)(object)enumInt;

    if (!data.HasFlag(val)) {
      var (canPurchase, minRank) =
        await ranks.CheckRank(player, Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = getCost(val);

      if (await eco.TryPurchase(executor, cost,
        item: $"title ({val.ToString().ToTitleCase()})") < 0) {
        return CommandResult.SUCCESS;
      }

      Unsafe.As<T, short>(ref data) |= Unsafe.As<T, short>(ref val);
      await gangStats.SetForGang(gang, statId, data);

      if (gangChat != null)
        await gangChat.SendGangChat(player, gang,
          localizer.Get(MSG.PERK_PURCHASED,
            $"{title} ({val.ToString().ToTitleCase()})"));
      return CommandResult.SUCCESS;
    }

    if (isOnlyGang) {
      var (canManage, required) =
        await ranks.CheckRank(player, Perm.MANAGE_PERKS);
      if (!canManage) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, required.Name));
        return CommandResult.SUCCESS;
      }

      Unsafe.As<T, short>(ref equipped) = Unsafe.As<T, short>(ref val);
      await gangStats.SetForGang(gang, statId, data);

      if (gangChat == null) return CommandResult.SUCCESS;

      await gangChat.SendGangChat(player, gang,
        localizer.Get(MSG.GANG_THING_SET, title, formatItem(val)));
      return CommandResult.SUCCESS;
    }

    await playerStats.SetForPlayer(executor, statId, val);
    executor.PrintToChat(
      $"{localizer.Get(MSG.PREFIX)}Set your {title.ToLower()} to {formatItem(val)}");
    return CommandResult.SUCCESS;
  }

  virtual protected string formatItem(T item) {
    return $"{item.ToString().ToTitleCase()}";
  }
}