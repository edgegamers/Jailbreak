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
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class MarkerCommandBehavior(IWardenCmdMarkerLocale markerLocale,
  IGenericCmdLocale generics, IBeamShapeRegistry registry,
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
    foreach (var type in registry.GetAllTypes()) {
      menu.AddMenuOption(type.ToFriendlyString(),
        (p, _) => handleMarkerTypeSelect(p, type));
    }

    menu.Open(player);
  }

  private void handleMarkerTypeSelect(CCSPlayerController player,
    BeamShapeType type) {
    var steam = player.SteamID;
    Task.Run(async () => {
      await markerSettings.SetTypeAsync(steam, type);
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        var value = type.ToFriendlyString();
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
    foreach (var color in registry.GetAllColors()) {
      menu.AddMenuOption(color.Key,
        (p, _) => handleMarkerColorSelect(p, color));
    }

    menu.Open(player);
  }

  private void handleMarkerColorSelect(CCSPlayerController player,
    KeyValuePair<string, Color> color) {
    var steam = player.SteamID;
    Task.Run(async () => {
      await markerSettings.SetColorAsync(steam, color.Key);
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        markerLocale.ColorChanged(color.Key).ToChat(player);
      });
    });
    MenuManager.CloseActiveMenu(player);
  }
}