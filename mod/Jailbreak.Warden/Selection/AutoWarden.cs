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
  
  private static readonly ConcurrentDictionary<ulong, bool> cachedCookies = new();
  private readonly HashSet<ulong> moved = [];
  private bool listenerRegistered;

  private static readonly FakeConVar<string> CV_AUTOWARDEN_FLAG =
    new("css_autowarden_flag", "Permission flag required to enable auto-Warden",
      "@ego/dssilver");
  
  private BasePlugin plugin = null!;
  private ICookie? cookie;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    
    TryLoadCookie();
    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    basePlugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
  }
  
  public void Dispose() { 
    if (listenerRegistered)
      plugin.DeregisterEventHandler<EventPlayerFootstep>(OnPlayerStep);
  }
  
  private void OnMapStart(string mapname) {
    // Attempt to load the cookie OnMapStart if it fails to load on plugin start
    // This can happen if the MAUL plugin is loaded *after* this plugin
    if (cookie == null) TryLoadCookie();
    else plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    if (listenerRegistered) return HookResult.Continue;
    Server.NextFrame(() => {
      plugin.RegisterEventHandler<EventPlayerFootstep>(OnPlayerStep);
      listenerRegistered = true;
      waitForInactiveSelection();
    });
    return HookResult.Continue;
  }
  
  private HookResult OnPlayerStep(EventPlayerFootstep @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) { return HookResult.Continue; }
    
    if (!player.IsReal() || !player.PawnIsAlive || moved.Contains(player.SteamID))
      return HookResult.Continue;

    if (player.Team != CsTeam.CounterTerrorist)
      return HookResult.Continue;

    if (!AdminManager.PlayerHasPermissions(player, CV_AUTOWARDEN_FLAG.Value))
      return HookResult.Continue;

    if (!cachedCookies.TryGetValue(player.SteamID, out var value)) {
      _ = Task.Run(async () => await populateCache(player, player.SteamID));
      return HookResult.Continue;
    }

    moved.Add(player.SteamID);
    selectionService.TryEnter(player);
    return HookResult.Continue;
  }

  [ConsoleCommand("css_autowarden")]
  public void Command_AutoWarden(CCSPlayerController? player, CommandInfo info) {
    if (player == null) return;
    
    if (!AdminManager.PlayerHasPermissions(player, CV_AUTOWARDEN_FLAG.Value)) {
      generic.NoPermissionMessage(CV_AUTOWARDEN_FLAG.Value).ToChat(player);
      return;
    }

    if (cookie == null) {
      locale.TogglingNotEnabled.ToChat(player);
      return;
    }

    var steam = player.SteamID;
    Task.Run(async () => {
      var value  = await cookie.Get(steam);
      var enable = value is not (null or "Y");
      await cookie.Set(steam, enable ? "Y" : "N");
      await Server.NextFrameAsync(() => {
        if (!player.IsValid) return;
        locale.AutoWardenToggled(enable).ToChat(player);
        cachedCookies[steam] = enable;
      });
    });
    
  }
  
  private void waitForInactiveSelection() {
    Server.RunOnTick(Server.TickCount + 5, () => {
      if (!selectionService.Active && listenerRegistered) {
        plugin.DeregisterEventHandler<EventPlayerFootstep>(OnPlayerStep);
        listenerRegistered = false;
      } else {
        waitForInactiveSelection();
      }
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
    var val = await cookie.Get(steam);
    cachedCookies[steam] = val is null or "Y";
    if (!cachedCookies[steam]) return;
    await Server.NextFrameAsync(() => {
      if (!player.IsValid) return;
      selectionService.TryEnter(player);
    });
  }
}