using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class SoccerCommandBehavior(IWardenService warden,
  IWardenLocale wardenLocale, IWardenCmdSoccerLocale locale) : IPluginBehavior {
  public static readonly FakeConVar<int> CV_MAX_SOCCERS =
    new("css_jb_max_soccers",
      "The maximum number of soccer balls that the warden can spawn", 3);

  private int soccerBalls;

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    soccerBalls = 0;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_soccer", "Spawn a soccer ball as the warden")]
  [ConsoleCommand("css_spawnball", "Spawn a soccer ball as the warden")]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      wardenLocale.NotWarden.ToChat(player);
      return;
    }

    if (soccerBalls >= CV_MAX_SOCCERS.Value) {
      locale.TooManySoccers.ToChat(player);
      return;
    }

    var ball =
      Utilities.CreateEntityByName<CPhysicsPropMultiplayer>(
        "prop_physics_multiplayer");
    if (ball == null || !ball.IsValid) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    var loc = player.Pawn.Value?.AbsOrigin;
    if (loc == null) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    ball.SetModel(
      "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
    ball.Teleport(loc);
    locale.SoccerSpawned.ToAllChat();
    ball.DispatchSpawn();
    soccerBalls++;
  }
}