﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;

namespace Jailbreak.SpecialDay.SpecialDays;

public class HideAndSeekDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractArmoryRestrictedDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.HNS;

  private HNSDayLocale Msg => (HNSDayLocale)Locale;
  public override SpecialDaySettings Settings => new HnsSettings();
  public override IView ArmoryReminder => Msg.StayInArmory;
  public ISDInstanceLocale Locale => new HNSDayLocale();

  // Set to -1 to not modify values
  public static readonly FakeConVar<int> CV_PRISONER_PRE_HEALTH = new(
    "jb_sd_hns_hide_hp_t", "Health to give to prisoners during HNS hide time",
    300, ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<int> CV_GUARD_PRE_HEALTH =
    new("jb_sd_hns_hide_hp_ct", "Health to give to guards during HNS hide time",
      150, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<int> CV_PRISONER_PRE_ARMOR = new(
    "jb_sd_hns_hide_armor_t", "Armor to give to prisoners during HNS hide time",
    200, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<int> CV_GUARD_PRE_ARMOR =
    new("jb_sd_hns_hide_armor_ct",
      "Armor to give to guards during HNS hide time", 300,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<int> CV_PRISONER_POST_HEALTH = new(
    "jb_sd_hns_seek_hp_t", "Health to give to prisoners during HNS seek time",
    300, ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(1, 1000));

  public static readonly FakeConVar<int> CV_GUARD_POST_HEALTH = new(
    "jb_sd_hns_seek_hp_ct", "Health to give to guards during HNS seek time", 25,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(1, 1000));

  public static readonly FakeConVar<int> CV_PRISONER_POST_ARMOR = new(
    "jb_sd_hns_seek_armor_t", "Armor to give to prisoners during HNS seek time",
    500, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<int> CV_GUARD_POST_ARMOR = new(
    "jb_sd_hns_seek_armor_ct", "Armor to give to guards during HNS seek time",
    -1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public static readonly FakeConVar<string> CV_GUARD_WEAPONS = new(
    "jb_sd_hns_weapons_ct",
    "List of weapons/items CTs may use, empty for no restrictions",
    string.Join(",", Tag.PISTOLS.Union(Tag.UTILITY)), ConVarFlags.FCVAR_NONE,
    new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<string> CV_PRISONER_WEAPONS = new(
    "jb_sd_hns_weapons_t",
    "List of weapons/items Ts may use, empty for no restrictions", "",
    ConVarFlags.FCVAR_NONE, new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<string> CV_SEEKER_TEAM =
    new("jb_sd_hns_seekers", "Team to assign as seekers and restrict to armory",
      "t", ConVarFlags.FCVAR_NONE, new TeamValidator(false));

  public static readonly FakeConVar<int> CV_SEEK_TIME =
    new("jb_sd_hns_seektime",
      "Duration in seconds to give the hiders time to hide", 45,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(5, 120));

  private CsTeam? SeekerTeam => TeamUtil.FromString(CV_SEEKER_TEAM.Value);

  private CsTeam? HiderTeam
    => (SeekerTeam ?? CsTeam.Terrorist) == CsTeam.Terrorist ?
      CsTeam.CounterTerrorist :
      CsTeam.Terrorist;

  public override void Setup() {
    if (SeekerTeam == null || HiderTeam == null) return;
    RestrictedTeam = SeekerTeam.Value;

    if (CV_SEEK_TIME.Value >= 10)
      Timers[10] += () => {
        foreach (var ct in PlayerUtil.FromTeam(SeekerTeam.Value))
          ct.SetSpeed(1.5f);

        Msg.DamageWarning(15).ToTeamChat(SeekerTeam.Value);
      };
    if (CV_SEEK_TIME.Value >= 25)
      Timers[25] += () => {
        foreach (var player in PlayerUtil.FromTeam(SeekerTeam.Value)) {
          player.SetSpeed(1.25f);
          EnableDamage(player);
        }
      };

    if (CV_SEEK_TIME.Value >= 30)
      Timers[30] += () => {
        foreach (var ct in PlayerUtil.FromTeam(SeekerTeam.Value))
          ct.SetSpeed(1.1f);
      };

    for (var offset = 15; offset < CV_SEEK_TIME.Value; offset += 15) {
      var beginsIn = CV_SEEK_TIME.Value - offset;
      Timers[CV_SEEK_TIME.Value - offset] +=
        () => Locale.BeginsIn(beginsIn).ToAllChat();
    }

    Timers[CV_SEEK_TIME.Value] += Execute;

    base.Setup();

    foreach (var player in PlayerUtil.FromTeam(SeekerTeam.Value))
      player.SetSpeed(2f);
  }

  public override void Execute() {
    if (SeekerTeam == null || HiderTeam == null) return;
    base.Execute();
    foreach (var player in PlayerUtil.GetAlive()) {
      var hp = (player.Team == CsTeam.Terrorist ?
        CV_PRISONER_POST_HEALTH :
        CV_GUARD_POST_HEALTH).Value;

      var armor = (player.Team == CsTeam.Terrorist ?
        CV_PRISONER_POST_ARMOR :
        CV_GUARD_POST_ARMOR).Value;

      if (hp != -1) player.SetHealth(hp);
      if (armor != -1) player.SetArmor(armor);
    }

    foreach (var ct in PlayerUtil.FromTeam(SeekerTeam.Value)) ct.SetSpeed(1);
  }

  public class HnsSettings : SpecialDaySettings {
    private readonly ISet<string>? cachedGuardWeapons, cachedPrisonerWeapons;

    public HnsSettings() {
      AllowLastRequests = true;
      TTeleport         = TeleportType.ARMORY;
      CtTeleport        = TeleportType.ARMORY;

      cachedGuardWeapons    = CV_GUARD_WEAPONS.Value.Split(",").ToHashSet();
      cachedPrisonerWeapons = CV_PRISONER_WEAPONS.Value.Split(",").ToHashSet();

      if (cachedGuardWeapons.Count == 0) cachedGuardWeapons       = null;
      if (cachedPrisonerWeapons.Count == 0) cachedPrisonerWeapons = null;
    }

    public override int InitialHealth(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        CV_PRISONER_PRE_HEALTH.Value :
        CV_GUARD_PRE_HEALTH.Value;
    }

    public override int InitialArmor(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        CV_PRISONER_PRE_ARMOR.Value :
        CV_GUARD_PRE_ARMOR.Value;
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        cachedPrisonerWeapons :
        cachedGuardWeapons;
    }
  }
}