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

public class ChickenCommandBehavior(IWardenService warden,
  IWardenLocale wardenLocale, IWardenCmdChickenLocale locale)
  : IPluginBehavior {
  public static readonly FakeConVar<int> CV_MAX_CHICKENS =
    new("css_jb_max_chickens",
      "The maximum number of chickens that the warden can spawn", 5);

  private int chickens;

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    chickens = 0;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_chicken", "Spawn a chicken as the warden")]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      wardenLocale.NotWarden.ToChat(player);
      return;
    }

    if (chickens >= CV_MAX_CHICKENS.Value) {
      locale.TooManyChickens.ToChat(player);
      return;
    }

    var chicken = Utilities.CreateEntityByName<CChicken>("chicken");
    if (chicken == null || !chicken.IsValid) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    var loc = player.Pawn.Value?.AbsOrigin;
    if (loc == null) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    chicken.Teleport(loc);
    locale.ChickenSpawned.ToAllChat();
    chicken.DispatchSpawn();
    chickens++;
  }
}