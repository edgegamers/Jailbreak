using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Utils;

namespace Jailbreak.Formatting.Extensions;

public static class ViewExtensions {
  public static FormatWriter ToWriter(this IView view) {
    var writer = new FormatWriter();

    view.Render(writer);

    return writer;
  }

  public static IView ToServerConsole(this IView view) {
    var writer = view.ToWriter();

    foreach (var s in writer.Plain) Server.PrintToConsole(s);

    return view;
  }

  public static IView ToAllConsole(this IView view) {
    Utilities.GetPlayers().ForEach(player => view.ToPlayerConsole(player));

    return view;
  }

  public static IView ToAllChat(this IView view) {
    Utilities.GetPlayers().ForEach(player => view.ToPlayerChat(player));

    return view;
  }

  public static IView ToAllCenter(this IView view) {
    Utilities.GetPlayers().ForEach(player => view.ToPlayerCenter(player));

    return view;
  }

  #region Individual

  public static IView ToPlayerConsole(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();

    foreach (var writerLine in writer.Plain) player.PrintToConsole(writerLine);

    return view;
  }

  public static IView ToPlayerChat(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();

    foreach (var writerLine in writer.Chat) player.PrintToChat(writerLine);

    return view;
  }

  public static IView ToPlayerCenter(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();
    var merged = string.Join('\n', writer.Plain);

    player.PrintToCenter(merged);

    return view;
  }

  public static IView ToPlayerCenterHtml(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();
    var merged = string.Join('\n', writer.Panorama);

    player.PrintToCenterHtml(merged);

    return view;
  }

  #endregion

  #region team

  public static IView ToTeamChat(this IView view, CsTeam team) {
    foreach (var player in PlayerUtil.FromTeam(team, false))
      view.ToPlayerChat(player);

    return view;
  }

  public static IView ToTeamCenter(this IView view, CsTeam team) {
    foreach (var player in PlayerUtil.FromTeam(team, false))
      view.ToPlayerCenter(player);

    return view;
  }

  public static IView ToTeamCenterHtml(this IView view, CsTeam team) {
    foreach (var player in PlayerUtil.FromTeam(team, false))
      view.ToPlayerCenterHtml(player);

    return view;
  }

  #endregion
}