using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Warden.Enums;

namespace Jailbreak.Warden.Commands;

public class MarkerCommandBehavior(IWardenCmdMarkerLocale markerLocale,
  IGenericCmdLocale generics,
  IWardenMarkerSettings markerSettings) : IPluginBehavior {
  public static readonly FakeConVar<string> CV_MARKER_CUSTOMIZATION_FLAG =
    new("css_marker_customization_flag",
      "Permission flag required to customize your maker", "@ego/dssilver");

  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) { plugin = basePlugin; }

  [ConsoleCommand("css_markertype")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_MarkerType(CCSPlayerController? player,
    CommandInfo command) {
    if (player == null) return;
    if (!AdminManager.PlayerHasPermissions(player,
      CV_MARKER_CUSTOMIZATION_FLAG.Value)) {
      generics.NoPermissionMessage(CV_MARKER_CUSTOMIZATION_FLAG.Value)
       .ToChat(player);
      return;
    }

    var menu = new CenterHtmlMenu("Marker Type", plugin);
    foreach (var type in MarkerShapeTypeExtensions.All()) {
      menu.AddMenuOption(type.ToDisplayName(),
        (p, _) => handleMarkerTypeSelect(p, type));
    }

    menu.Open(player);
  }

  private void handleMarkerTypeSelect(CCSPlayerController player,
    MarkerShapeType type) {
    var steam = player.SteamID;
    Task.Run(async () => {
      await markerSettings.SetTypeAsync(steam, type);
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        var value = type.ToDisplayName();
        markerLocale.TypeChanged(value).ToChat(player);
      });
    });
    MenuManager.CloseActiveMenu(player);
  }

  [ConsoleCommand("css_markercolor")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_MarkerColor(CCSPlayerController? player,
    CommandInfo command) {
    if (player == null) return;
    if (!AdminManager.PlayerHasPermissions(player,
      CV_MARKER_CUSTOMIZATION_FLAG.Value)) {
      generics.NoPermissionMessage(CV_MARKER_CUSTOMIZATION_FLAG.Value)
       .ToChat(player);
      return;
    }

    var menu = new CenterHtmlMenu("Marker Color", plugin);
    foreach (var color in MarkerColorExtensions.All()) {
      menu.AddMenuOption(color.ToDisplayName(),
        (p, _) => handleMarkerColorSelect(p, color));
    }

    menu.Open(player);
  }

  private void handleMarkerColorSelect(CCSPlayerController player,
    MarkerColor color) {
    var steam = player.SteamID;
    Task.Run(async () => {
      await markerSettings.SetColorAsync(steam, color.ToDisplayName());
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        markerLocale.ColorChanged(color.ToDisplayName()).ToChat(player);
      });
    });
    MenuManager.CloseActiveMenu(player);
  }
}