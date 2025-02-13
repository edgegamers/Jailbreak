using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestRebelCommand(ILastRequestManager lastRequestManager,
  ILastRequestRebelManager lastRequestRebelManager, ILRLocale messages)
  : IPluginBehavior {
  private readonly Dictionary<int, int> rebellerHealths = [];

  public void Start(BasePlugin basePlugin) {
    basePlugin.RegisterListener<Listeners.OnEntityParentChanged>(OnDrop);
  }

  private void OnDrop(CEntityInstance entity, CEntityInstance newparent) {
    if (!entity.IsValid || !Tag.WEAPONS.Contains(entity.DesignerName)) return;

    var weapon = Utilities.GetEntityFromIndex<CCSWeaponBase>((int)entity.Index);
    if (weapon == null
      || weapon.PrevOwner.Get()?.OriginalController.Get() == null)
      return;

    var owner = weapon.PrevOwner.Get()?.OriginalController.Get();
    if (owner == null || newparent.IsValid) return;

    if (!rebellerHealths.TryGetValue(owner.Slot, out var hp)) return;
    if (owner.Pawn.Value != null)
      owner.SetHealth(Math.Min(hp, owner.Pawn.Value.Health));

    rebellerHealths.Remove(owner.Slot);
  }

  [ConsoleCommand("css_rebel", "Rebel during last request as a prisoner")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_Rebel(CCSPlayerController? rebeller, CommandInfo info) {
    if (rebeller == null || !rebeller.IsReal()) return;
    if (!LastRequestRebelManager.CV_REBEL_ON.Value) {
      messages.LastRequestRebelDisabled().ToChat(rebeller);
      return;
    }

    if (rebeller.Team != CsTeam.Terrorist) {
      messages.CannotLastRequestRebelCt().ToChat(rebeller);
      return;
    }

    if (!lastRequestManager.IsLREnabled || !rebeller.PawnIsAlive) {
      messages.LastRequestNotEnabled().ToChat(rebeller);
      return;
    }

    if (lastRequestManager.IsInLR(rebeller)
      || lastRequestRebelManager.IsInLRRebelling(rebeller.Slot)) {
      messages.CannotLR("You are already in an LR").ToChat(rebeller);
      return;
    }

    if (rebeller.Pawn.Value != null)
      rebellerHealths[rebeller.Slot] = rebeller.Pawn.Value.Health;
    lastRequestRebelManager.StartLRRebelling(rebeller);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    lastRequestRebelManager.ClearLRRebelling();
    rebellerHealths.Clear();
    return HookResult.Continue;
  }
}