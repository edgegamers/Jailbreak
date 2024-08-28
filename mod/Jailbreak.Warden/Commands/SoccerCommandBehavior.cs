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

  private int soccers = 0;

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    soccers = 0;
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

    if (soccers >= CV_MAX_SOCCERS.Value) {
      locale.TooManySoccers.ToChat(player);
      return;
    }

    var chicken =
      Utilities.CreateEntityByName<CPhysicsPropMultiplayer>(
        "prop_physics_multiplayer");
    if (chicken == null || !chicken.IsValid) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    chicken.SetModel(
      "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
    chicken.Teleport(player.AbsOrigin);
    locale.SoccerSpawned.ToAllChat();
    chicken.DispatchSpawn();
  }
}