using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.SpecialDay;

public class SpecialDaySettings {
  public enum TeleportType {
    /// <summary>
    ///   Don't do any teleportation
    /// </summary>
    NONE,

    /// <summary>
    ///   Teleport all prisoners to random CT spawns
    /// </summary>
    ARMORY,

    /// <summary>
    ///   Teleport all prisoners to random CT spawns
    /// </summary>
    ARMORY_STACKED,

    /// <summary>
    ///   Teleport all CTs to random T spawns
    /// </summary>
    CELL,

    /// <summary>
    ///   Teleport all CTs to a random T spawn
    /// </summary>
    CELL_STACKED,

    /// <summary>
    ///   Teleport all players randomly across the map
    /// </summary>
    RANDOM
  }

  public bool AllowLastRequests = true;

  /// <summary>
  ///   If true, teleport all players regardless of where they should've
  ///   spawned in.
  ///   Eg: if Teleport is set to ARMORY, CTs would not normally be
  ///   teleported. If this is set to true, they will be.
  /// </summary>
  public bool ForceTeleportAll = false;

  public bool FreezePlayers = true;
  public bool RespawnPlayers = true;

  /// <summary>
  ///   If true, all players will be immune from damage at the beginning.
  /// </summary>
  public bool StartInvulnerable = true;

  /// <summary>
  ///   If true, will strip all players down to their knife at the beginning.
  /// </summary>
  public bool StripToKnife = true;

  public TeleportType Teleport = TeleportType.NONE;

  public Dictionary<string, object> ConVarValues { get; } = new();

  public ISet<string> AllowedWeapons { get; } = Tag.WEAPONS.ToHashSet();

  public virtual Func<int> RoundTime => () => 60 * 5;

  public virtual float FreezeTime(CCSPlayerController player) { return 3; }

  public SpecialDaySettings WithFriendlyFire() {
    ConVarValues["mp_teammates_are_enemies"]     = true;
    ConVarValues["ff_damage_reduction_bullets"]  = 1;
    ConVarValues["ff_damage_reduction_grenades"] = 1;
    ConVarValues["ff_damage_reduction_other"]    = 1;
    return this;
  }

  public SpecialDaySettings WithRespawns(CsTeam? team = CsTeam.None) {
    switch (team) {
      case CsTeam.None:
      case CsTeam.Spectator:
        ConVarValues["mp_respawn_on_death_ct"] = true;
        ConVarValues["mp_respawn_on_death_t"]  = true;
        break;
      case CsTeam.Terrorist:
        ConVarValues["mp_respawn_on_death_t"] = true;
        break;
      case CsTeam.CounterTerrorist:
        ConVarValues["mp_respawn_on_death_ct"] = true;
        break;
    }

    return this;
  }
}