using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.SpecialDay;

public class SpecialDaySettings {
  public enum TeleportType {
    /// <summary>
    /// Don't do any teleportation
    /// </summary>
    NONE,

    /// <summary>
    /// Teleport all prisoners to random CT spawns
    /// </summary>
    ARMORY,

    /// <summary>
    /// Teleport all prisoners to random CT spawns
    /// </summary>
    ARMORY_STACKED,

    /// <summary>
    /// Teleport all CTs to random T spawns
    /// </summary>
    CELL,

    /// <summary>
    /// Teleport all CTs to a random T spawn
    /// </summary>
    CELL_STACKED,

    /// <summary>
    /// Teleport all players randomly across the map
    /// </summary>
    RANDOM
  }

  public Dictionary<string, object> ConVarValues { get; } = new();

  public ISet<string> AllowedWeapons { get; } = Tag.WEAPONS.ToHashSet();

  public TeleportType Teleport = TeleportType.NONE;

  /// <summary>
  /// If true, teleport all players regardless of where they should've
  /// spawned in.
  ///
  /// Eg: if Teleport is set to ARMORY, CTs would not normally be
  /// teleported. If this is set to true, they will be.
  /// </summary>
  public bool ForceTeleportAll = false;

  public virtual float FreezeTime(CCSPlayerController player) => 3;

  public virtual Func<int> RoundTime => () => 60 * 5;

  public bool FreezePlayers = true;
  public bool AllowLastRequests = true;

  /// <summary>
  /// If true, all players will be immune from damage at the beginning.
  /// </summary>
  public bool StartInvulnerable = true;

  /// <summary>
  /// If true, will strip all players down to their knife at the beginning.
  /// </summary>
  public bool StripToKnife = true;

  public SpecialDaySettings WithFriendlyFire() {
    this.ConVarValues["mp_teammates_are_enemies"]     = true;
    this.ConVarValues["ff_damage_reduction_bullets"]  = 1;
    this.ConVarValues["ff_damage_reduction_grenades"] = 1;
    this.ConVarValues["ff_damage_reduction_other"]    = 1;
    return this;
  }

  public SpecialDaySettings WithRespawns(CsTeam? team = CsTeam.None) {
    switch (team) {
      case CsTeam.None:
      case CsTeam.Spectator:
        this.ConVarValues["mp_respawn_on_death_ct"] = true;
        this.ConVarValues["mp_respawn_on_death_t"]  = true;
        break;
      case CsTeam.Terrorist:
        this.ConVarValues["mp_respawn_on_death_t"] = true;
        break;
      case CsTeam.CounterTerrorist:
        this.ConVarValues["mp_respawn_on_death_ct"] = true;
        break;
    }

    return this;
  }
}