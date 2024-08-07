using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Microsoft.VisualBasic.CompilerServices;

namespace Jailbreak.SpecialDay.SpecialDays;

public class GunGameDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private readonly IList<string> weaponProgression = [];

  private readonly IList<string> BEST = [
    "weapon_awp", "weapon_scar20", "weapon_g3sg1"
  ];

  private readonly IList<string> GREAT = [
    "weapon_ak47", "weapon_m4a1", "weapon_m4a1_silencer",
  ];

  private readonly IList<string> GOOD = [
    "weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9",
    "weapon_p90", "weapon_ump45", "weapon_negev"
  ];

  private readonly IList<string> OKAY = [
    "weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014",
    "weapon_ssg08"
  ];

  private readonly IList<string> BAD = [
    "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock",
    "weapon_hkp2000", "weapon_p250", "weapon_usp_silencer", "weapon_tec9",
    "weapon_cz75a", "weapon_revolver"
  ];

  private readonly IList<string> LAST = ["weapon_knife", "weapon_taser"];

  private readonly IDictionary<int, int> progressions =
    new Dictionary<int, int>();

  private ShuffleBag<Vector>? spawns;

  public ISDInstanceLocale Locale => new GunDayLocale();

  public override SDType Type => SDType.GUNGAME;
  public override SpecialDaySettings Settings => new GunGameSettings();
  private IGunDayLocale msg => (IGunDayLocale)Locale;

  public override void Setup() {
    base.Setup();
    spawns =
      new ShuffleBag<Vector>(getAtLeastRandom(Utilities.GetPlayers().Count));

    Plugin.RegisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
    Plugin.RegisterEventHandler<EventPlayerSpawn>(OnRespawn, HookMode.Pre);

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
    player.GiveNamedItem(weaponProgression[index]);
    if (spawns != null) player.Teleport(spawns.GetNext());
    return HookResult.Continue;
  }

  private HookResult OnDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player   = @event.Userid;
    var attacker = @event.Attacker;
    info.DontBroadcast = true;
    if (player == null || !player.IsValid) return HookResult.Continue;
    int index;
    if (attacker == null || !attacker.IsValid) {
      if (!progressions.TryGetValue(player.Slot, out index)) index = 0;
      if (index > 0) {
        index--;
        msg.DemotedDueToSuicide.ToChat(player);
        progressions[player.Slot] = index;
      }

      return HookResult.Continue;
    }

    var attackerProgress =
      progressions.TryGetValue(attacker.Slot, out index) ? index : 0;
    if (attackerProgress == weaponProgression.Count - 1) {
      info.DontBroadcast = false;
      msg.PlayerWon(attacker).ToAllChat();
      foreach (var p in PlayerUtil.GetAlive()) {
        if (p.Slot == attacker.Slot) continue;
        p.Teleport(attacker);
      }

      attacker.RemoveWeapons();
      attacker.GiveNamedItem("weapon_negev");
      attacker.GiveNamedItem("weapon_knife");
      DisableDamage(attacker);

      Server.ExecuteCommand("mp_respawn_on_death_t 0");
      Server.ExecuteCommand("mp_respawn_on_death_ct 0");

      Plugin.DeregisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
      return HookResult.Continue;
    }


    if (@event.Weapon.Contains("knife")) {
      progressions[attacker.Slot] = Math.Min(attackerProgress + 2,
        weaponProgression.Count - 1);
    } else {
      progressions[attacker.Slot] = Math.Min(attackerProgress + 1,
        weaponProgression.Count - 1);
    }

    if (attackerProgress == weaponProgression.Count - 1)
      msg.PlayerOnLastPromotion(attacker).ToAllChat();

    msg.PromotedTo(weaponProgression[attackerProgress + 1],
        weaponProgression.Count - attackerProgress - 1)
     .ToChat(attacker);

    attacker.RemoveWeapons();
    attacker.GiveNamedItem(weaponProgression[attackerProgress + 1]);
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

  public class GunGameSettings : SpecialDaySettings {
    public GunGameSettings() {
      CtTeleport                              = TeleportType.RANDOM;
      TTeleport                               = TeleportType.RANDOM;
      FreezePlayers                           = false;
      ConVarValues["mp_respawn_immunitytime"] = 5.0f;
      WithFriendlyFire();
      WithRespawns();
    }
  }
}