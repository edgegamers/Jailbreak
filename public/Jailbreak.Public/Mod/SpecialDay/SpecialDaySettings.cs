using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.SpecialDay;

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
    RANDOM,

    /// <summary>
    ///   Pick a random teleport on the map and teleport all players to it
    /// </summary>
    RANDOM_STACKED
  }

  public bool AllowLastGuard = false;

  public bool AllowLastRequests = false;

  public bool AllowRebels = false;

  public TeleportType CtTeleport = TeleportType.NONE;

  public bool FreezePlayers = true;
  public bool OpenCells = true;
  public bool RespawnPlayers = true;

  [Obsolete("No longer used", true)]
  public bool RestrictWeapons = false;

  /// <summary>
  ///   If true, all players will be immune from damage at the beginning.
  /// </summary>
  public bool StartInvulnerable = true;

  /// <summary>
  ///   If true, will strip all players down to their knife at the beginning.
  /// </summary>
  public bool StripToKnife = true;

  public TeleportType TTeleport = TeleportType.NONE;

  public Dictionary<string, object> ConVarValues { get; } = new();

  public virtual Func<int> RoundTime => () => 60 * 5;

  public virtual ISet<string>? AllowedWeapons(CCSPlayerController player) {
    return null;
  }

  public virtual float FreezeTime(CCSPlayerController player) { return 3; }

  /// <summary>
  ///   The health to set a given player to at the beginning of the round.
  ///   -1 to not change the player's health.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public virtual int InitialHealth(CCSPlayerController player) { return 100; }

  /// <summary>
  ///   The health to set a given player to at the beginning of the round.
  ///   -1 to not change the player's health.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public virtual int InitialMaxHealth(CCSPlayerController player) {
    return 100;
  }

  /// <summary>
  ///   The armor to set a given player to at the beginning of the round.
  ///   -1 to not change the player's armor.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public virtual int InitialArmor(CCSPlayerController player) { return 0; }

  public SpecialDaySettings WithFriendlyFire() {
    ConVarValues["mp_teammates_are_enemies"]    = true;
    ConVarValues["ff_damage_reduction_bullets"] = 1.0f;
    ConVarValues["ff_damage_reduction_grenade"] = 1.0f;
    ConVarValues["ff_damage_reduction_other"]   = 1.0f;
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

  public SpecialDaySettings WithAutoBhop() {
    ConVarValues["sv_enablebunnyhopping"] = true;
    ConVarValues["sv_autobunnyhopping"]   = true;
    return this;
  }
}