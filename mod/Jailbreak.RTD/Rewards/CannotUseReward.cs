using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CannotUseReward : IRTDReward {
  private readonly ImmutableHashSet<string> blockedWeapons;
  private readonly BasePlugin plugin;

  public CannotUseReward(BasePlugin plugin, WeaponType blocked) : this(plugin,
    blocked.GetItems().ToArray()) {
    NameShort = blocked.ToString().ToTitleCase();
  }

  public CannotUseReward(BasePlugin plugin, params string[] weapons) {
    this.plugin    = plugin;
    blockedWeapons = weapons.ToImmutableHashSet();
    NameShort = string.Join(", ",
      blockedWeapons.Select(s => s.GetFriendlyWeaponName()));
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnCanAcquire,
      HookMode.Pre);
    plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
  }

  private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnCanAcquire,
      HookMode.Pre);
    blockedIDs.Clear();
    plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
    return HookResult.Continue;
  }

  private HookResult OnCanAcquire(DynamicHook hook) {
    var player = hook.GetParam<CCSPlayer_ItemServices>(0)
     .Pawn.Value.Controller.Value?.As<CCSPlayerController>();
    var data = VirtualFunctions.GetCSWeaponDataFromKey.Invoke(-1,
      hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString());

    if (player == null || !player.IsValid) return HookResult.Continue;

    var method = hook.GetParam<AcquireMethod>(2);
    if (method != AcquireMethod.PickUp) return HookResult.Continue;

    if (player.UserId == null || !blockedIDs.Contains(player.UserId.Value))
      return HookResult.Continue;

    if (!blockedWeapons.Contains(data.Name)) return HookResult.Continue;

    hook.SetReturn(AcquireResult.NotAllowedByMode);
    return HookResult.Handled;
  }

  public string Name => $"Cannot Use {NameShort}";
  public string NameShort { get; }

  public string Description
    => $"You will not be able to use {NameShort} next round.";

  private readonly HashSet<int> blockedIDs = [];

  public bool GrantReward(CCSPlayerController player) {
    if (player.UserId == null) return false;
    blockedIDs.Add(player.UserId.Value);
    return true;
  }
}