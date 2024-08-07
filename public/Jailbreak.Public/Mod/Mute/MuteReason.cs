namespace Jailbreak.Public.Mod.Mute;

public enum MuteReason {
  /// <summary>
  /// An admin invoked the peace
  /// </summary>
  ADMIN,

  /// <summary>
  /// The warden invoked the peace
  /// </summary>
  WARDEN_INVOKED,

  /// <summary>
  /// The first warden of the round has been assigned
  /// </summary>
  INITIAL_WARDEN,

  /// <summary>
  /// A new warden has been assigned
  /// </summary>
  WARDEN_TAKEN
}