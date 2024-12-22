using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public class GunGameDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private readonly IList<string> BAD = [
    "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock",
    "weapon_hkp2000", "weapon_p250", "weapon_usp_silencer", "weapon_tec9",
    "weapon_cz75a", "weapon_revolver"
  ];

  private readonly IList<string> BEST = [
    "weapon_awp", "weapon_scar20", "weapon_g3sg1"
  ];

  private readonly IList<string> GOOD = [
    "weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9",
    "weapon_p90", "weapon_ump45", "weapon_negev"
  ];

  private readonly IList<string> GREAT = [
    "weapon_ak47", "weapon_m4a1", "weapon_m4a1_silencer"
  ];

  private readonly IList<string> LAST = ["weapon_knife", "weapon_taser"];

  private readonly IList<string> OKAY = [
    "weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014",
    "weapon_ssg08"
  ];

  private readonly IDictionary<int, int> progressions =
    new Dictionary<int, int>();

  private readonly IList<string> weaponProgression = [];

  private ShuffleBag<Vector>? spawns;

  public override SDType Type => SDType.GUNGAME;

  private IGunDayLocale msg => (IGunDayLocale)Locale;
  public override SpecialDaySettings Settings => new GunGameSettings();

  public ISDInstanceLocale Locale => new GunDayLocale();

  public override void Setup() {
    Timers[5]  += () => Locale.BeginsIn(5).ToAllChat();
    Timers[10] += Execute;

    base.Setup();
    var mgr = Provider.GetService<IZoneManager>();
    spawns =
      new ShuffleBag<Vector>(
        MapUtil.GetRandomSpawns(Utilities.GetPlayers().Count, mgr));

    Plugin.RegisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
    Plugin.RegisterEventHandler<EventPlayerSpawn>(OnRespawn);

    BEST.Shuffle();
    GREAT.Shuffle();
    GOOD.Shuffle();
    OKAY.Shuffle();
    BAD.Shuffle();
    LAST.Shuffle();

    weaponProgression.Add(BEST[0]);
    weaponProgression.Add(GREAT[0]);
    weaponProgression.Add(GREAT[1]);
    weaponProgression.Add(GOOD[0]);
    weaponProgression.Add(GOOD[2]);
    weaponProgression.Add(GOOD[3]);
    weaponProgression.Add(OKAY[0]);
    weaponProgression.Add(BAD[0]);
    weaponProgression.Add(BAD[1]);
    weaponProgression.Add(LAST[0]);
  }

  public override void Execute() {
    base.Execute();
    Locale.BeginsIn(0).ToAllChat();
    foreach (var player in PlayerUtil.GetAlive()) {
      progressions[player.Slot] = 0;
      player.RemoveWeapons();
      player.GiveNamedItem(weaponProgression[0]);
      player.GiveNamedItem("weapon_knife");
    }
  }

  private HookResult OnRespawn(EventPlayerSpawn @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    if (!progressions.TryGetValue(player.Slot, out var index)) index = 0;
    Plugin.AddTimer(0.1f, () => {
      if (!player.IsValid || player.Pawn.Value == null || !player.Pawn.IsValid)
        return;
      player.GiveNamedItem(weaponProgression[index]);
      if (spawns != null) player.Pawn.Value.Teleport(spawns.GetNext());
    });
    return HookResult.Continue;
  }

  private HookResult OnDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player   = @event.Userid;
    var attacker = @event.Attacker;
    info.DontBroadcast = true;
    if (player == null || !player.IsValid) return HookResult.Continue;
    int playerIndex;
    if (!progressions.TryGetValue(player.Slot, out playerIndex))
      playerIndex = 0;
    if (attacker == null || !attacker.IsValid) return HookResult.Continue;
    if (attacker.Slot == player.Slot) return HookResult.Continue;

    var attackerProgress =
      progressions.TryGetValue(attacker.Slot, out var attackerIndex) ?
        attackerIndex :
        0;
    if (attackerProgress == weaponProgression.Count - 1) {
      info.DontBroadcast = false;
      msg.PlayerWon(attacker).ToAllChat();
      foreach (var p in PlayerUtil.GetAlive()) {
        if (p.Slot == attacker.Slot) continue;
        p.Teleport(attacker);
        p.RemoveWeapons();
      }

      attacker.SetSpeed(2f);
      attacker.RemoveWeapons();
      attacker.GiveNamedItem("weapon_negev");
      attacker.GiveNamedItem("weapon_knife");
      DisableDamage(attacker);

      Server.ExecuteCommand("mp_respawn_on_death_t 0");
      Server.ExecuteCommand("mp_respawn_on_death_ct 0");

      RoundUtil.SetTimeRemaining(Math.Min(RoundUtil.GetTimeRemaining(), 30));

      Plugin.DeregisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
      Plugin.RemoveListener<Listeners.OnTick>(OnTick);
      return HookResult.Continue;
    }

    attackerProgress += 1;
    if (@event.Weapon.Contains("knife")) {
      attackerProgress += 1;
      msg.DemotedDueToKnife.ToChat(player);
      progressions[player.Slot] = Math.Max(playerIndex - 1, 0);
    }

    attackerProgress = Math.Min(attackerProgress, weaponProgression.Count - 1);

    progressions[attacker.Slot] = attackerProgress;

    if (attackerProgress == weaponProgression.Count - 1)
      msg.PlayerOnLastPromotion(attacker).ToAllChat();

    msg.PromotedTo(weaponProgression[attackerProgress].GetFriendlyWeaponName(),
        weaponProgression.Count - attackerProgress)
     .ToChat(attacker);

    attacker.RemoveWeapons();
    attacker.GiveNamedItem(weaponProgression[attackerProgress]);
    attacker.GiveNamedItem("weapon_knife");
    return HookResult.Continue;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    Plugin.DeregisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
    Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnRespawn);

    return result;
  }

  override protected void OnTick() {
    foreach (var player in PlayerUtil.GetAlive()) {
      var weapons = allowedWeapons(player);
      disableWeapon(player, weapons);
    }
  }

  private ISet<string> allowedWeapons(CCSPlayerController player) {
    var progress = progressions.TryGetValue(player.Slot, out var index) ?
      index :
      0;

    var weapon  = weaponProgression[progress];
    var allowed = new HashSet<string> { weapon, "weapon_knife" };

    if (Tag.RIFLES.Contains(weapon))
      allowed = allowed.Union(Tag.RIFLES).ToHashSet();

    if (Tag.PISTOLS.Contains(weapon))
      allowed = allowed.Union(Tag.PISTOLS).ToHashSet();

    return allowed.Union(Tag.UTILITY).ToHashSet();
  }

  private void disableWeapon(CCSPlayerController player,
    ICollection<string> allowed) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (allowed.Contains(activeWeapon.DesignerName)) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
    activeWeapon.NextPrimaryAttackTick   = Server.TickCount + 500;
  }

  public class GunGameSettings : SpecialDaySettings {
    public GunGameSettings() {
      CtTeleport                              = TeleportType.RANDOM;
      TTeleport                               = TeleportType.RANDOM;
      FreezePlayers                           = false;
      ConVarValues["mp_respawn_immunitytime"] = 5.0f;
      ConVarValues["mp_death_drop_gun"]       = 0;
      RestrictWeapons                         = true;
      WithFriendlyFire();
      WithRespawns();
    }
  }
}