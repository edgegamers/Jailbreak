using System.Collections.Concurrent;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using MAULActainShared.plugin.models;

namespace Jailbreak.Warden.Selection;

public class AutoWarden(IWardenSelectionService selectionService,
  IWardenLocale locale, IGenericCmdLocale generic) : IPluginBehavior {
  
  private static readonly ConcurrentDictionary<ulong, bool> CACHED_COOKIES = new();
  
  private static readonly FakeConVar<string> CV_AUTOWARDEN_FLAG =
    new("css_autowarden_flag", "Permission flag required to enable auto-Warden",
      "@ego/dssilver");
  private static readonly FakeConVar<float> CV_AUTOWARDEN_DELAY_INTERVAL =
    new("css_autowarden_delay_interval", "The amount of time in seconds to wait after round start to queue users with auto-warden enabled for warden",
      5f);
  
  private BasePlugin plugin = null!;
  private ICookie? cookie;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    
    TryLoadCookie();
    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    basePlugin.RegisterEventHandler<EventRoundPoststart>(OnRoundStart);
  }
  
  public void Dispose() { }
  
  private void OnMapStart(string mapname) {
    // Attempt to load the cookie OnMapStart if it fails to load on plugin start
    // This can happen if the MAUL plugin is loaded *after* this plugin
    if (cookie == null) TryLoadCookie();
    else plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  private HookResult OnRoundStart(EventRoundPoststart @event, GameEventInfo info) {
    plugin.AddTimer(CV_AUTOWARDEN_DELAY_INTERVAL.Value, () => {
      foreach (var player in Utilities.GetPlayers()
       .Where(p => p.Team == CsTeam.CounterTerrorist 
          && p.IsReal()
          && p.PawnIsAlive
          && AdminManager.PlayerHasPermissions(p, CV_AUTOWARDEN_FLAG.Value))) {
        
        if (player.PlayerPawn.Value == null
          || !player.PlayerPawn.Value.HasMovedSinceSpawn) 
          continue;
        
        var steam = player.SteamID;
        if (!CACHED_COOKIES.ContainsKey(steam))
          Task.Run(async () => await populateCache(player, steam));

        if (!CACHED_COOKIES.TryGetValue(steam, out var value) || !value)
          continue;
        selectionService.TryEnter(player);
        locale.JoinRaffle.ToChat(player);
      }
    });
    return HookResult.Continue;
  }

  [ConsoleCommand("css_autowarden")]
  public void Command_AutoWarden(CCSPlayerController? player, CommandInfo info) {
    if (player == null) return;
    if (!AdminManager.PlayerHasPermissions(player, CV_AUTOWARDEN_FLAG.Value)) {
      generic.NoPermissionMessage(CV_AUTOWARDEN_FLAG.Value).ToChat(player);
      return;
    }
    if (cookie == null) { locale.TogglingNotEnabled.ToChat(player); return; }

    var steam = player.SteamID;
    Task.Run(async () => {
      var cur    = await cookie.Get(steam);
      var enable = cur != "Y";
      await cookie.Set(steam, enable ? "Y" : "N");
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        locale.AutoWardenToggled(enable).ToChat(player);
        CACHED_COOKIES[steam] = enable;
      });
    });
    
  }

  private void TryLoadCookie() {
    Task.Run(async () => {
      if (API.Actain != null)
        cookie = await API.Actain.getCookieService()
         .RegClientCookie("jb_warden_auto");
    });
  }

  private async Task populateCache(CCSPlayerController player, ulong steam) {
    if (cookie == null) return;
    var val     = await cookie.Get(steam);
    var enabled = val == "Y";
    CACHED_COOKIES[steam] = enabled;
    if (!enabled) return;
    await Server.NextFrameAsync(() => {
      if (!player.IsValid) return;
      selectionService.TryEnter(player);
      locale.JoinRaffle.ToChat(player);
    });
  }
}