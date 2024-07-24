using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogEntityListeners(IRichLogService logs) : IPluginBehavior {
  [EntityOutputHook("func_button", "OnPressed")]
  public HookResult OnButtonPressed(CEntityIOOutput output, string name,
    CEntityInstance activator, CEntityInstance caller, CVariant value,
    float delay) {
    if (!activator.TryGetController(out var player)) return HookResult.Continue;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);

    logs.Append(logs.Player(player),
      $"pressed a button: {ent?.Entity?.Name ?? "Unlabeled"} -> {output.Connections?.TargetDesc ?? "None"}");
    return HookResult.Continue;
  }

  [EntityOutputHook("func_breakable", "OnBreak")]
  public HookResult OnBreakableBroken(CEntityIOOutput output, string name,
    CEntityInstance activator, CEntityInstance caller, CVariant value,
    float delay) {
    if (!activator.TryGetController(out var player)) return HookResult.Continue;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);

    logs.Append(logs.Player(player),
      $"broke an entity: {ent?.Entity?.Name ?? "Unlabeled"} -> {output.Connections?.TargetDesc ?? "None"}");
    return HookResult.Continue;
  }
}