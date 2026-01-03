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
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;
using MAULActainShared.plugin.models;

namespace Jailbreak.Warden.Commands;

public class MarkerCommandBehavior(IWardenCmdMarkerLocale markerLocale,
  IGenericCmdLocale generics, IBeamShapeRegistry registry) : IPluginBehavior {
  public static readonly FakeConVar<string> CV_MARKER_CUSTOMIZATION_FLAG =
    new("css_marker_customization_flag",
      "Permission flag required to customize your maker", "@ego/dssilver");

  private BasePlugin plugin = null!;
  private ICookie? typeCookie;
  private ICookie? colorCookie;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    tryLoadCookie();
    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
  }

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

    if (typeCookie == null) {
      markerLocale.ChangingNotEnabled.ToChat(player);
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
    if (typeCookie == null) return;

    var steam = player.SteamID;
    Task.Run(async () => {
      var value = type.ToFriendlyString();
      await typeCookie.Set(steam, value);
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        markerLocale.TypeChanged(value).ToChat(player);
      });
    });
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

    if (colorCookie == null) {
      markerLocale.ChangingNotEnabled.ToChat(player);
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
    if (colorCookie == null) return;
    
    var steam = player.SteamID;
    Task.Run(async () => {
      var value = color.Key;
      await colorCookie.Set(steam, value);
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        markerLocale.ColorChanged(value).ToChat(player);
      });
    });
  }

  private void tryLoadCookie() {
    Task.Run(async () => {
      if (API.Actain != null) {
        typeCookie = await API.Actain.getCookieService()
         .RegClientCookie("jb_marker_type");
        colorCookie = await API.Actain.getCookieService()
         .RegClientCookie("jb_marker_color");
      }
    });
  }

  private void OnMapStart(string mapname) {
    // Attempt to load the cookie OnMapStart if it fails to load on plugin start
    // This can happen if the MAUL plugin is loaded *after* this plugin
    if (typeCookie == null)
      tryLoadCookie();
    else
      plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }
}