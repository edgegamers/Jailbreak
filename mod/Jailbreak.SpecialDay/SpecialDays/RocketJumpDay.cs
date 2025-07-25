using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.SpecialDay.SpecialDays;

public class RocketJumpDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public static readonly FakeConVar<float> CV_BULLET_SPEED = new(
    "css_jb_rj_bullet_speed", "Speed of the projectile bullet.", 1250.0f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(100f, 5000f));

  public static readonly FakeConVar<float> CV_MAX_DISTANCE = new(
    "css_jb_rj_max_distance", "Max distance to apply rocketjump.", 160.0f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 1000f));

  public static readonly FakeConVar<float> CV_CLOSE_JUMP_DISTANCE = new(
    "css_jb_rj_close_jump_distance",
    "Max distance that causes a full force jump.", 37.0f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 1000f));

  public static readonly FakeConVar<float> CV_JUMP_FORCE_MAIN = new(
    "css_jb_rj_jump_force_main", "Base jump push strength.", 270.0f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0f, 2000f));

  public static readonly FakeConVar<float> CV_JUMP_FORCE_UP = new(
    "css_jb_rj_jump_force_up", "Vertical boost on rocketjump.", 8.0f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0f, 500f));

  public static readonly FakeConVar<float> CV_JUMP_FORCE_FORWARD = new(
    "css_jb_rj_jump_force_forward", "Forward scale on rocketjump.", 1.2f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0f, 10f));

  public static readonly FakeConVar<float> CV_JUMP_FORCE_BACKWARD = new(
    "css_jb_rj_jump_force_backward", "Backward scale on rocketjump.", 1.25f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0f, 10f));

  public static readonly FakeConVar<float> CV_RUN_FORCE_MAIN = new(
    "css_jb_rj_run_force_main", "Extra boost if running.", 0.8f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0f, 10f));

  public static readonly FakeConVar<bool> CV_PROJ_INHERIT_PLAYER_VELOCITY = new(
    "css_jb_rj_proj_inherit_player_velocity",
    "Whether the projectile inherits player velocity. "
    + "True allows for easier rocket jumps at the cost of 'funky' shot paths when trying to shoot a player");

  public static readonly FakeConVar<float> CV_PROJ_DAMAGE = new(
    "css_jb_rj_proj_damage", "The damage caused by projectile explosion", 34f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 200f));

  public static readonly FakeConVar<float> CV_PROJ_DAMAGE_RADIUS = new(
    "css_jb_rj_proj_damage_radius",
    "The radius of the explosion caused by projectile", 150f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 1000f));

  public static readonly FakeConVar<float> CV_PROJ_GRAVITY = new(
    "css_jb_rj_proj_gravity", "The gravity of the projectile.", 0.001f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.001f, 2000f));

  private const int GE_FIRE_BULLETS_ID = 452;

  // Thank you https://github.com/ipsvn/cs2-Rocketjump/tree/master
  private readonly MemoryFunctionVoid<nint, nint> touch =
    new("55 48 89 E5 41 54 49 89 F4 53 48 8B 87");

  private readonly Dictionary<CHEGrenadeProjectile, CCSPlayerController> shots =
    new();

  private readonly HashSet<CCSPlayerPawn> jumping = [];

  public override SDType Type => SDType.RJ;
  public override SpecialDaySettings Settings => new RocketJumpSettings();

  public ISDInstanceLocale Locale
    => new SoloDayLocale("Rocket Jump",
      "Your shotgun is now an RPG that fires grenades — shoot the ground to launch! Mid-air knives hit hard!");

  public override void Setup() {
    Plugin.HookUserMessage(GE_FIRE_BULLETS_ID, fireBulletsUmHook);
    touch.Hook(CBaseEntity_Touch, HookMode.Pre);
    Plugin.RegisterEventHandler<EventWeaponFire>(onWeaponFire);
    Plugin.RegisterEventHandler<EventPlayerHurt>(onHurt, HookMode.Pre);
    Plugin.RegisterListener<Listeners.OnTick>(onTick);

    Timers[10] += () => Locale.BeginsIn(10).ToAllChat();
    Timers[15] += () => Locale.BeginsIn(5).ToAllChat();
    Timers[20] += Execute;

    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
        player.GiveNamedItem("weapon_nova");
    }
    base.Execute();
  }

  override protected HookResult OnEnd(EventRoundEnd ev, GameEventInfo info) {
    Plugin.UnhookUserMessage(GE_FIRE_BULLETS_ID, fireBulletsUmHook);
    touch.Unhook(CBaseEntity_Touch, HookMode.Pre);
    Plugin.DeregisterEventHandler<EventWeaponFire>(onWeaponFire);
    Plugin.DeregisterEventHandler<EventPlayerHurt>(onHurt, HookMode.Pre);
    Plugin.RemoveListener<Listeners.OnTick>(onTick);
    return base.OnEnd(ev, info);
  }

  private static HookResult fireBulletsUmHook(UserMessage um) {
    um.Recipients.Clear();
    return HookResult.Continue;
  }

  private HookResult CBaseEntity_Touch(DynamicHook hook) {
    var decoy = hook.GetParam<CHEGrenadeProjectile>(0);
    if (decoy.DesignerName != "hegrenade_projectile")
      return HookResult.Continue;

    var owner = decoy.OwnerEntity.Value?.As<CCSPlayerPawn>();
    if (owner == null || owner.DesignerName != "player")
      return HookResult.Continue;

    var bulletOrigin = decoy.AbsOrigin;
    var pawnOrigin   = owner.AbsOrigin;
    if (bulletOrigin == null || pawnOrigin == null) return HookResult.Continue;

    var eyeOrigin = owner.GetEyeOrigin();
    var distance  = Vector3.Distance(bulletOrigin.Into(), pawnOrigin.Into());

    shots.Remove(decoy, out _);
    decoy.DetonateTime = 0f;
    doJump(owner, distance, bulletOrigin.Into(), eyeOrigin);

    return HookResult.Handled;
  }

  private HookResult onWeaponFire(EventWeaponFire @event, GameEventInfo info) {
    var controller = @event.Userid;
    if (controller == null) return HookResult.Continue;

    var weapon = @event.Weapon;
    if (weapon != "weapon_nova") return HookResult.Continue;

    var pawn   = controller.PlayerPawn.Value;
    var origin = pawn?.AbsOrigin;
    if (pawn == null || origin == null) return HookResult.Continue;
    pawn.GetEyeForward(10.0f, out var forwardDir, out var targetPos);

    var realBulletVelocity = forwardDir * CV_BULLET_SPEED.Value;
    var addedBulletVelocity = CV_PROJ_INHERIT_PLAYER_VELOCITY.Value ?
      pawn.AbsVelocity.Into() + realBulletVelocity :
      realBulletVelocity;
    shootBullet(controller, targetPos, addedBulletVelocity,
      new Vector3(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z));

    return HookResult.Continue;
  }

  private HookResult onHurt(EventPlayerHurt @event, GameEventInfo info) {
    var attackerPawn = @event.Attacker?.PlayerPawn.Value;
    var hurtPawn     = @event.Userid?.PlayerPawn.Value;

    if (attackerPawn == null || hurtPawn == null) return HookResult.Continue;
    if (@event.Weapon.Contains("grenade")) return HookResult.Continue;

    if (!@event.Weapon.Contains("knife")) return HookResult.Handled;
    if (!jumping.Contains(attackerPawn)) return HookResult.Continue;

    hurtPawn.Health = 1;
    Utilities.SetStateChanged(hurtPawn, "CBaseEntity", "m_iHealth");

    return HookResult.Continue;
  }

  private void onTick() {
    foreach (var player in jumping.Where(p => p.OnGroundLastTick).ToList())
      jumping.Remove(player);
  }

  private void shootBullet(CCSPlayerController controller, Vector3 origin,
    Vector3 velocity, Vector3 angle) {
    var pawn = controller.PlayerPawn.Value;
    if (pawn == null) return;

    var decoy =
      Utilities
       .CreateEntityByName<CHEGrenadeProjectile>("hegrenade_projectile");
    if (decoy == null) return;

    decoy.OwnerEntity.Raw = pawn.EntityHandle.Raw;
    decoy.Damage          = CV_PROJ_DAMAGE.Value;
    decoy.DmgRadius       = CV_PROJ_DAMAGE_RADIUS.Value;
    decoy.DispatchSpawn();
    decoy.AcceptInput("InitializeSpawnFromWorld", pawn, pawn);
    Schema.SetSchemaValue(decoy.Handle, "CBaseGrenade", "m_hThrower",
      pawn.EntityHandle.Raw);
    decoy.GravityScale = CV_PROJ_GRAVITY.Value;
    decoy.DetonateTime = 9999f;

    unsafe {
      decoy.Teleport(new Vector((nint)(&origin)), new QAngle((nint)(&angle)),
        new Vector((nint)(&velocity)));
    }

    shots[decoy] = controller;
  }

  private void doJump(CCSPlayerPawn pawn, float distance, Vector3 bulletOrigin,
    Vector3 pawnOrigin) {
    if (distance >= CV_MAX_DISTANCE.Value) return;

    var down                  = false;
    var direction             = Vector3.Normalize(pawnOrigin - bulletOrigin);
    if (direction.Z < 0) down = true;

    var pawnVelocity = pawn.AbsVelocity;
    var movementDir =
      Vector3.Normalize(new Vector3(pawnVelocity.X, pawnVelocity.Y, 0));

    var dot = Vector3.Dot(direction, movementDir);
    var scale = dot >= 0 ?
      CV_JUMP_FORCE_FORWARD.Value :
      CV_JUMP_FORCE_BACKWARD.Value;

    var velocity      = direction * CV_JUMP_FORCE_MAIN.Value;
    var totalVelocity = (pawnVelocity.Into() + velocity) * scale;
    pawnVelocity.Z  = 0.0f;
    totalVelocity.Z = 0.0f;

    if (pawn.OnGroundLastTick) totalVelocity *= CV_RUN_FORCE_MAIN.Value;

    var forceUp = CV_JUMP_FORCE_UP.Value * (CV_MAX_DISTANCE.Value - distance);
    if (distance > CV_CLOSE_JUMP_DISTANCE.Value)
      if (totalVelocity.Z > 0.0f)
        totalVelocity.Z = 1000.0f + forceUp;
      else
        totalVelocity.Z += forceUp;
    else
      totalVelocity.Z += forceUp / 1.37f;
    if (down) velocity.Z *= -1.0f;
    unsafe { pawn.Teleport(null, null, new Vector((nint)(&totalVelocity))); }

    jumping.Add(pawn);
  }

  public class RocketJumpSettings : SpecialDaySettings {
    public RocketJumpSettings() {
      CtTeleport   = TeleportType.RANDOM;
      TTeleport    = TeleportType.RANDOM;
      StripToKnife = true;
      WithFriendlyFire();

      ConVarValues["sv_infinite_ammo"]                 = 1;
      ConVarValues["mp_death_drop_gun"]                = 0;
      ConVarValues["ff_damage_reduction_grenade_self"] = 0;
      ConVarValues["sv_falldamage_scale"]              = 0;
    }
    
    public override float FreezeTime(CCSPlayerController player) { return 1; }
  }
}