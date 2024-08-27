using CounterStrikeSharp.API;
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

  private HNSDayLocale msg => (HNSDayLocale)Locale;
  public override SpecialDaySettings Settings => new HNSSettings(this);
  public override IView ArmoryReminder => msg.StayInArmory;
  public ISDInstanceLocale Locale => new HNSDayLocale();

  // Set to -1 to not modify values
  public readonly FakeConVar<int> CvPrisonerPreHealth = new(
    "jb_sd_hns_hide_hp_t", "Health to give to prisoners during HNS hide time",
    300, ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(-1, 1000));

  public readonly FakeConVar<int> CvGuardPreHealth = new("jb_sd_hns_hide_hp_ct",
    "Health to give to guards during HNS hide time", 150,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public readonly FakeConVar<int> CvPrisonerPreArmor = new(
    "jb_sd_hns_hide_armor_t", "Armor to give to prisoners during HNS hide time",
    200, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public readonly FakeConVar<int> CvGuardPreArmor =
    new("jb_sd_hns_hide_armor_ct",
      "Armor to give to guards during HNS hide time", 300,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public readonly FakeConVar<int> CvPrisonerPostHealth = new(
    "jb_sd_hns_seek_hp_t", "Health to give to prisoners during HNS seek time",
    300, ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(1, 1000));

  public readonly FakeConVar<int> CvGuardPostHealth = new(
    "jb_sd_hns_seek_hp_ct", "Health to give to guards during HNS seek time", 25,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<int>(1, 1000));

  public readonly FakeConVar<int> CvPrisonerPostArmor = new(
    "jb_sd_hns_seek_armor_t", "Armor to give to prisoners during HNS seek time",
    500, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public readonly FakeConVar<int> CvGuardPostArmor = new(
    "jb_sd_hns_seek_armor_ct", "Armor to give to guards during HNS seek time",
    -1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(-1, 1000));

  public readonly FakeConVar<string> CvGuardWeapons = new(
    "jb_sd_hns_weapons_ct",
    "List of weapons/items CTs may use, empty for no restrictions",
    string.Join(",", Tag.PISTOLS.Union(Tag.UTILITY)));

  public readonly FakeConVar<string> CvPrisonerWeapons = new(
    "jb_sd_hns_weapons_t",
    "List of weapons/items Ts may use, empty for no restrictions", "");

  public override void Setup() {
    Timers[10] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
        ct.SetSpeed(1.5f);

      msg.DamageWarning(15).ToTeamChat(CsTeam.CounterTerrorist);

      Locale.BeginsIn(35).ToAllChat();
    };
    Timers[25] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetSpeed(1.25f);
        EnableDamage(ct);
      }
    };
    Timers[30] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
        ct.SetSpeed(1.1f);
      Locale.BeginsIn(15).ToAllChat();
    };
    Timers[45] += Execute;

    base.Setup();

    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetSpeed(2f);
  }

  public override void Execute() {
    base.Execute();
    foreach (var player in PlayerUtil.GetAlive()) {
      var hp = (player.Team == CsTeam.Terrorist ?
        CvPrisonerPostHealth :
        CvGuardPostHealth).Value;

      var armor = (player.Team == CsTeam.Terrorist ?
        CvPrisonerPostArmor :
        CvGuardPostArmor).Value;

      if (hp != -1) player.SetHealth(hp);
      if (armor != -1) player.SetArmor(armor);
    }

    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetSpeed(1);
  }

  public class HNSSettings : SpecialDaySettings {
    private readonly HideAndSeekDay day;
    private readonly ISet<string>? cachedGuardWeapons, cachedPrisonerWeapons;

    public HNSSettings(HideAndSeekDay day) {
      this.day          = day;
      AllowLastRequests = true;
      TTeleport         = TeleportType.ARMORY;
      CtTeleport        = TeleportType.ARMORY;

      cachedGuardWeapons = day.CvGuardWeapons.Value.Split(",").ToHashSet();
      cachedPrisonerWeapons =
        day.CvPrisonerWeapons.Value.Split(",").ToHashSet();

      if (cachedGuardWeapons.Count == 0) cachedGuardWeapons       = null;
      if (cachedPrisonerWeapons.Count == 0) cachedPrisonerWeapons = null;
    }

    public override int InitialHealth(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        day.CvPrisonerPreHealth.Value :
        day.CvGuardPreHealth.Value;
    }

    public override int InitialArmor(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        day.CvPrisonerPreArmor.Value :
        day.CvGuardPreArmor.Value;
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ?
        cachedPrisonerWeapons :
        cachedGuardWeapons;
    }
  }
}