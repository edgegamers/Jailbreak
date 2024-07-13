using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenService {
  public CCSPlayerController? Warden { get; }

  /// <summary>
  ///   Whether or not a warden is currently assigned
  /// </summary>
  public bool HasWarden { get; }

  public bool IsWarden(CCSPlayerController? player) {
    if (player == null || !player.IsReal()) return false;
    return HasWarden && Warden != null && Warden.Slot == player.Slot;
  }

  public bool TrySetWarden(CCSPlayerController warden);

  public bool TryRemoveWarden(bool isPass = false);
}