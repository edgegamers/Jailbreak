using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Utils;

#pragma warning disable CS0618 // Type or member is obsolete

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
    view.ToConsole(Utilities.GetPlayers().ToArray());
    return view;
  }

  public static IView ToAllChat(this IView view) {
    view.ToChat(Utilities.GetPlayers().ToArray());
    return view;
  }

  public static IView ToAllCenter(this IView view) {
    view.ToCenter(Utilities.GetPlayers().ToArray());
    return view;
  }

  #region Individual

  [Obsolete("Use ToConsole instead")]
  public static IView ToPlayerConsole(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();

    foreach (var writerLine in writer.Plain) player.PrintToConsole(writerLine);

    return view;
  }

  [Obsolete("Use ToChat instead")]
  public static IView ToPlayerChat(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();

    foreach (var writerLine in writer.Chat) player.PrintToChat(writerLine);

    return view;
  }

  [Obsolete("Use ToCenter instead")]
  public static IView ToPlayerCenter(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();
    var merged = string.Join('\n', writer.Plain);

    player.PrintToCenter(merged);

    return view;
  }

  [Obsolete("Use ToCenterHtml instead")]
  public static IView ToPlayerCenterHtml(this IView view,
    CCSPlayerController player) {
    if (!player.IsReal() || player.IsBot) return view;

    var writer = view.ToWriter();
    var merged = string.Join('\n', writer.Panorama);

    player.PrintToCenterHtml(merged);

    return view;
  }

  #endregion

  #region Team

  public static IView ToTeamChat(this IView view, CsTeam team) {
    foreach (var player in PlayerUtil.FromTeam(team, false))
      view.ToChat(player);

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

  #region params

  public static IView ToConsole(this IView view,
    params CCSPlayerController[] players) {
    foreach (var player in players) view.ToPlayerConsole(player);

    return view;
  }

  public static IView ToChat(this IView view,
    params CCSPlayerController[] players) {
    foreach (var player in players) view.ToPlayerChat(player);

    return view;
  }

  public static IView ToCenter(this IView view,
    params CCSPlayerController[] players) {
    foreach (var player in players) view.ToPlayerCenter(player);

    return view;
  }

  #endregion
}