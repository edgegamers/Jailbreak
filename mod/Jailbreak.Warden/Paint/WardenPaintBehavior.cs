using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Rainbow;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;
using WardenPaintColorPerk;

namespace Jailbreak.Warden.Paint;

public class WardenPaintBehavior(IWardenService wardenService,
  IServiceProvider provider) : IPluginBehavior {
  private readonly IRainbowColorizer? colorizer =
    provider.GetService<IRainbowColorizer>();

  private WardenPaintColor?[] colors = new WardenPaintColor?[65];
  private bool[] fetched = new bool[65];
  private Vector? lastPosition;
  private BasePlugin? parent;

  public void Start(BasePlugin basePlugin) {
    parent = basePlugin;
    basePlugin.RegisterListener<Listeners.OnTick>(paint);
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    colors  = new WardenPaintColor?[65];
    fetched = new bool[65];
    return HookResult.Continue;
  }

  private void paint() {
    if (!wardenService.HasWarden) return;

    var warden = wardenService.Warden;
    if (warden == null || !warden.IsReal()) return;

    if ((warden.Buttons & PlayerButtons.Use) == 0) return;

    var position = RayTrace.FindRayTraceIntersection(warden);
    if (position == null) return;

    var start = lastPosition ?? position;
    start = start.Clone();

    if (lastPosition != null
      && position.DistanceSquared(lastPosition) < 25 * 25)
      return;

    lastPosition = position;
    if (start.DistanceSquared(position) > 150 * 150) start = position;

    if (parent == null)
      throw new NullReferenceException("Parent plugin is null");

    var color = getColor(warden);
    var line  = new BeamLine(parent, start.Clone(), position.Clone());
    line.SetColor(color);
    line.SetWidth(1.5f);
    line.Draw(30);
  }

  private Color getColor(CCSPlayerController player) {
    if (player.Pawn.Value == null || player.PlayerPawn.Value == null)
      return Color.White;
    var pawn = player.Pawn.Value;
    if (pawn == null || !pawn.IsValid) return Color.White;

    var color = colors[player.Index];
    if (color != null) {
      if (color == WardenPaintColor.RAINBOW)
        return colorizer?.GetRainbow() ?? Color.White;
      if ((color & WardenPaintColor.RANDOM) != 0)
        return color.Value.PickRandom() ?? Color.White;
      return color.Value.GetColor() ?? Color.White;
    }

    if (fetched[player.Index]) return Color.White;
    fetched[player.Index] = true;
    var wrapper = new PlayerWrapper(player);
    Task.Run(async () => {
      color                = await fetchColor(wrapper);
      colors[player.Index] = color;
    });

    return Color.White;
  }

  private async Task<WardenPaintColor> fetchColor(PlayerWrapper player) {
    var gangs       = API.Gangs?.Services.GetService<IGangManager>();
    var playerStats = API.Gangs?.Services.GetService<IPlayerStatManager>();
    var players     = API.Gangs?.Services.GetService<IPlayerManager>();
    var gangStats   = API.Gangs?.Services.GetService<IGangStatManager>();

    if (gangs == null || playerStats == null || players == null
      || gangStats == null)
      return WardenPaintColor.DEFAULT;

    var playerColors = await playerStats.GetForPlayer<WardenPaintColor>(
      player.Steam, WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    var gangPlayer = await players.GetPlayer(player.Steam);

    if (gangPlayer?.GangId == null) return WardenPaintColor.DEFAULT;

    var gang = await gangs.GetGang(gangPlayer.GangId.Value);
    if (gang == null) return WardenPaintColor.DEFAULT;

    var available = await gangStats.GetForGang<WardenPaintColor>(gang,
      WardenPaintColorPerk.WardenPaintColorPerk.STAT_ID);

    if (playerColors == WardenPaintColor.RANDOM)
      return playerColors | available;

    return playerColors & available;
  }
}