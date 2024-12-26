// ReSharper disable once CheckNamespace

public static class Tag {
  /// <summary>
  /// Items that can backstab
  /// </summary>
  public static readonly IReadOnlySet<string> KNIVES = new HashSet<string>([
    "weapon_knife", "weapon_knife_bayonet", "weapon_knife_butterfly",
    "weapon_knife_canis", "weapon_knife_cord", "weapon_knife_css",
    "weapon_knife_falchion", "weapon_knife_flip", "weapon_knife_gut",
    "weapon_knife_gypsy_jackknife", "weapon_knife_karambit",
    "weapon_knife_m9_bayonet", "weapon_knife_push", "weapon_knife_skeleton",
    "weapon_knife_stiletto", "weapon_knife_survival_bowie",
    "weapon_knife_tactical", "weapon_knife_talon", "weapon_knife_ursus",
    "weapon_bayonet"
  ]);

  /// <summary>
  ///   Items that are thrown and exist in the grenade slot
  /// </summary>
  public static readonly IReadOnlySet<string> GRENADES = new HashSet<string>([
    "weapon_decoy", "weapon_firebomb", "weapon_flashbang", "weapon_hegrenade",
    "weapon_incgrenade", "weapon_molotov", "weapon_smokegrenade",
    "weapon_tagrenade", "weapon_frag"
  ]);

  /// <summary>
  ///   Items that do not shoot bullets
  /// </summary>
  public static readonly IReadOnlySet<string> UTILITY = new HashSet<string>([
      "weapon_healthshot", "item_assaultsuit", "item_kevlar",
      "weapon_diversion",
      "weapon_breachcharge", "weapon_bumpmine", "weapon_c4", "weapon_tablet",
      "weapon_taser", "weapon_shield", "weapon_snowball",
    ]).Union(GRENADES)
   .ToHashSet();

  public static readonly IReadOnlySet<string> SNIPERS = new HashSet<string>([
    "weapon_awp", "weapon_ssg08", "weapon_scar20", "weapon_g3sg1"
  ]);

  public static readonly IReadOnlySet<string> PISTOLS = new HashSet<string>([
    "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock",
    "weapon_hkp2000", "weapon_p250", "weapon_usp_silencer", "weapon_tec9",
    "weapon_cz75a", "weapon_revolver"
  ]);

  public static readonly IReadOnlySet<string> SHOTGUNS = new HashSet<string>([
    "weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014"
  ]);

  public static readonly IReadOnlySet<string> SMGS = new HashSet<string>([
    "weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9",
    "weapon_p90", "weapon_ump45"
  ]);

  public static readonly IReadOnlySet<string> HEAVY = new HashSet<string>([
    "weapon_negev", "weapon_m249"
  ]);

  public static readonly IReadOnlySet<string> RIFLES = new HashSet<string>([
      "weapon_ak47", "weapon_aug", "weapon_famas", "weapon_galilar",
      "weapon_m4a1", "weapon_m4a1_silencer", "weapon_sg556"
    ]).Union(SNIPERS)
   .Union(SHOTGUNS)
   .Union(SMGS)
   .Union(HEAVY)
   .ToHashSet();

  public static readonly IReadOnlySet<string> GUNS = RIFLES.Union(PISTOLS)
   .Union(RIFLES)
   .ToHashSet();

  public static readonly IReadOnlySet<string> WEAPONS =
    GUNS.Union(KNIVES).ToHashSet();
}