using System.Numerics;
using System.Runtime.InteropServices;
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
    "css_jb_rj_proj_damage", "The damage caused by projectile explosion", 65f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 200f));

  public static readonly FakeConVar<float> CV_PROJ_DAMAGE_RADIUS = new(
    "css_jb_rj_proj_damage_radius",
    "The radius of the explosion caused by projectile", 225f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(1f, 1000f));

  public static readonly FakeConVar<float> CV_PROJ_GRAVITY = new(
    "css_jb_rj_proj_gravity", "The gravity of the projectile.", 0.001f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.001f, 2000f));

  private const int GE_FIRE_BULLETS_ID = 452;
  private const int TOUCH_VTABLE_INDEX = 148;
  private const int HE_GRENADE_ITEM_DEF_INDEX = 44;
  private const float PROJECTILE_SPAWN_OFFSET = 24.0f;
  private const float PROJECTILE_FAILSAFE_LIFETIME = 10.0f;

  // CreateEntityByName does not run the native HE projectile factory logic
  // that arms the projectile's grenade-think/detonation state. These signatures
  // are from a CounterStrikeSharp implementation updated July 30, 2026.
  private static readonly MemoryFunctionWithReturn<
    nint, nint, nint, nint, nint, int, CHEGrenadeProjectile> HE_GRENADE_CREATE =
      new(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "55 4C 89 C1 48 89 E5 41 57 49 89 FF 41 56 49 89 D6 48 89 F2 48 89 FE 41 55"
        : "48 89 ? 24 ? 48 89 ? 24 ? 48 89 ? 24 ? 57 48 83 EC ? 48 8B ? 24 ? 49 8B F8 4C 8B C2 0F 29 ? 24 ? 48 8B D1 48 8B D9 48 8D 0D ? ? ? ? 4C 8B CD E8 ? ? ? ? F3 0F 10 0D ? ? ? ? 48 8B C8 48 8B F0 E8 ? ? ? ? 48 8B D7 48 8B CE");

  // Resolve Touch from a live HE projectile so derived overrides are hooked too.
  private VirtualFunctionVoid<CHEGrenadeProjectile, CBaseEntity>? grenadeTouch;

  private readonly HashSet<nint> rocketProjectiles = [];
  private readonly HashSet<CCSPlayerPawn> jumping = [];
  private readonly Dictionary<ulong, float> nextNova = new();

  public override SDType Type => SDType.ROCKETJUMP;
  public override SpecialDaySettings Settings => new RocketJumpSettings();

  public ISDInstanceLocale Locale
    => new SoloDayLocale("Rocket Jump",
      "Your shotgun is now an RPG that fires grenades! "
      + "Shoot the ground to launch! " + "Mid-air knives Insta-kill!");

  public override void Setup() {
    Plugin.HookUserMessage(GE_FIRE_BULLETS_ID, fireBulletsUmHook);
    Plugin.RegisterEventHandler<EventWeaponFire>(onWeaponFire);
    Plugin.RegisterListener<Listeners.OnPlayerTakeDamagePre>(onHurt);
    Plugin.RegisterListener<Listeners.OnTick>(onTick);

    Timers[10] += () => Locale.BeginsIn(10).ToAllChat();
    Timers[15] += () => Locale.BeginsIn(5).ToAllChat();
    Timers[20] += () => {
      Execute();
      Locale.BeginsIn(0).ToAllChat();
    };

    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      player.SetArmor(0);
      player.GiveNamedItem("weapon_knife");
      player.GiveNamedItem("weapon_nova");
    }

    base.Execute();
  }

  override protected HookResult OnEnd(EventRoundEnd ev, GameEventInfo info) {
    Plugin.UnhookUserMessage(GE_FIRE_BULLETS_ID, fireBulletsUmHook);
    grenadeTouch?.Unhook(CBaseEntity_Touch, HookMode.Pre);
    grenadeTouch = null;
    Plugin.DeregisterEventHandler<EventWeaponFire>(onWeaponFire);
    Plugin.RemoveListener<Listeners.OnPlayerTakeDamagePre>(onHurt);
    Plugin.RemoveListener<Listeners.OnTick>(onTick);

    // Delay to avoid mutation during hook execution.
    Server.NextFrameAsync(() => {
      jumping.Clear();
      rocketProjectiles.Clear();
      nextNova.Clear();
    });

    return base.OnEnd(ev, info);
  }

  /// <summary>
  ///   Clears recipients of the Nova bullet pellets to hide it from everyone.
  ///   Give cleaner shot effect and removes unecessary rendering
  /// </summary>
  private HookResult fireBulletsUmHook(UserMessage um) {
    um.Recipients.Clear();
    return HookResult.Continue;
  }

  /// <summary>
  ///   Handles when the grenade touches something.
  ///   Triggers a rocket jump for its owner if nearby.
  ///   Uses CHEGrenadeProjectile for built-in AoE, visibility, and raycast-like behavior.
  ///   This is prefered b/c using a raycast would require custom logic for:
  ///   -Damage radius simulation, Entity filtering, Visual/audio, feedback Manual hit registration
  /// </summary>
  private HookResult CBaseEntity_Touch(DynamicHook hook) {
    var projectile = hook.GetParam<CHEGrenadeProjectile>(0);
    if (!projectile.IsValid || !rocketProjectiles.Contains(projectile.Handle))
      return HookResult.Continue;

    var owner = projectile.Thrower.Value
      ?? projectile.OwnerEntity.Value?.As<CCSPlayerPawn>();
    if (owner == null || !owner.IsValid)
      return HookResult.Continue;

    // Do not detonate against the owner while the projectile is leaving the
    // player's collision hull.
    var other = hook.GetParam<CBaseEntity>(1);
    if (other.IsValid && other.Handle == owner.Handle)
      return HookResult.Continue;

    var bulletOrigin = projectile.AbsOrigin;
    if (bulletOrigin == null) return HookResult.Continue;

    // Touch can fire more than once. Consume this projectile before scheduling
    // the explosion so the jump and damage only happen once.
    rocketProjectiles.Remove(projectile.Handle);

    var ownerOrigin = owner.GetEyeOrigin();
    var impactOrigin = bulletOrigin.ToVec3();
    var distance = Vector3.Distance(impactOrigin, ownerOrigin);

    doJump(owner, distance, impactOrigin, ownerOrigin);

    // Native-created HE grenades have their detonation think armed. Current
    // CounterStrikeSharp examples force an immediate HE explosion with zero,
    // rather than Server.CurrentTime or an entity input.
    projectile.TicksAtZeroVelocity = 100;
    projectile.DetonateTime = 0f;

    // Repeat on the next frame in case the impact happened between grenade
    // think intervals. Do not call the "Detonate" input: it is not required
    // by recent HE examples and is not reliable for this projectile class.
    Server.NextFrame(() => {
      if (!projectile.IsValid) return;
      projectile.TicksAtZeroVelocity = 100;
      projectile.DetonateTime = 0f;
    });

    return HookResult.Continue;
  }

  /// <summary>
  ///   Detects Nova shots and spawns a grenade projectile in the direction the player is aiming.
  /// </summary>
  private HookResult onWeaponFire(EventWeaponFire @event, GameEventInfo info) {
    var controller = @event.Userid;
    if (controller == null) return HookResult.Continue;

    var weapon = @event.Weapon;
    if (weapon != "weapon_nova") return HookResult.Continue;

    var sid = controller.SteamID;
    var now = Server.CurrentTime;

    if (nextNova.TryGetValue(sid, out var next) && now < next)
      return HookResult.Continue;

    nextNova[sid] = now + 0.82f;

    var pawn = controller.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid || pawn.AbsOrigin == null)
      return HookResult.Continue;

    // The old GetEyeForward helper used AbsOrigin as pitch/yaw/roll, which made
    // projectile direction depend on the player's map coordinates.
    var eyeAngles = pawn.EyeAngles;
    var angleVector = new Vector3(eyeAngles.X, eyeAngles.Y, eyeAngles.Z);
    angleVector.AngleVectors(out var forwardDir, out _, out _);

    if (forwardDir.LengthSquared() < 0.0001f)
      return HookResult.Continue;

    forwardDir = Vector3.Normalize(forwardDir);

    var eyeOrigin = pawn.GetEyeOrigin();
    var spawnOrigin = eyeOrigin + forwardDir * PROJECTILE_SPAWN_OFFSET;
    var projectileVelocity = forwardDir * CV_BULLET_SPEED.Value;

    if (CV_PROJ_INHERIT_PLAYER_VELOCITY.Value)
      projectileVelocity += pawn.AbsVelocity.ToVec3();

    shootBullet(controller, spawnOrigin, projectileVelocity,
      new QAngle(eyeAngles.X, eyeAngles.Y, eyeAngles.Z));

    return HookResult.Continue;
  }

  /// <summary>
  ///   Makes knife hits lethal only if the attacker is airborne via rocket jump.
  ///   Nullifies Nova Pellet Damage
  ///   Passes Grenades Per Usual
  /// </summary>
  private HookResult onHurt(CCSPlayerPawn player, CTakeDamageInfo info) {
    var attacker   = info.Attacker.Value?.As<CCSPlayerPawn>();
    var weaponName = info.Ability.Value?.As<CCSWeaponBase>().VData?.Name;

    if (attacker == null || weaponName == null) return HookResult.Continue;

    if (weaponName.Contains("grenade")) return HookResult.Continue;

    if (!weaponName.Contains("knife") && !weaponName.Contains("bayonet"))
      return HookResult.Handled;

    if (jumping.Contains(attacker)) info.Damage = 200f;
    return HookResult.Continue;
  }

  /// <summary>
  ///   Continuously removes players from the "jumping" list once they land.
  /// </summary>
  private void onTick() {
    foreach (var player in jumping.Where(p => p.OnGroundLastTick).ToList())
      jumping.Remove(player);
  }

  /// <summary>
  ///   Spawns and launches a CHEGrenadeProjectile with explosive properties like radius and damage.
  /// </summary>
  private void shootBullet(CCSPlayerController controller, Vector3 origin,
    Vector3 velocity, QAngle rotation) {
    var pawn = controller.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;

    var pos = new Vector(origin.X, origin.Y, origin.Z);
    var vel = new Vector(velocity.X, velocity.Y, velocity.Z);

    // Use the native factory. CreateEntityByName makes a projectile that can
    // move and collide, but its HE detonation think may never be armed.
    var projectile = HE_GRENADE_CREATE.Invoke(
      pos.Handle,
      rotation.Handle,
      vel.Handle,
      vel.Handle,
      pawn.Handle,
      HE_GRENADE_ITEM_DEF_INDEX);

    if (projectile == null || !projectile.IsValid) return;

    ensureTouchHook(projectile);

    projectile.TeamNum             = pawn.TeamNum;
    projectile.Thrower.Raw         = pawn.EntityHandle.Raw;
    projectile.OriginalThrower.Raw = pawn.EntityHandle.Raw;
    projectile.OwnerEntity.Raw     = pawn.EntityHandle.Raw;

    projectile.Damage    = CV_PROJ_DAMAGE.Value;
    projectile.DmgRadius = CV_PROJ_DAMAGE_RADIUS.Value;

    projectile.InitialPosition.X = pos.X;
    projectile.InitialPosition.Y = pos.Y;
    projectile.InitialPosition.Z = pos.Z;
    projectile.InitialVelocity.X = vel.X;
    projectile.InitialVelocity.Y = vel.Y;
    projectile.InitialVelocity.Z = vel.Z;
    projectile.Teleport(pos, rotation, vel);

    projectile.GravityScale = CV_PROJ_GRAVITY.Value;
    projectile.DetonateTime =
      Server.CurrentTime + PROJECTILE_FAILSAFE_LIFETIME;

    rocketProjectiles.Add(projectile.Handle);
  }

  private void ensureTouchHook(CHEGrenadeProjectile projectile) {
    if (grenadeTouch != null) return;

    // Resolve slot 148 from the concrete projectile instance. Hooking the
    // CBaseEntity symbol alone can miss a derived Touch override.
    grenadeTouch = new(projectile, TOUCH_VTABLE_INDEX);
    grenadeTouch.Hook(CBaseEntity_Touch, HookMode.Pre);
  }

  /// <summary>
  ///   Calculates and applies rocket jump force based on distance and direction.
  ///   Combines player velocity with a directional push, scaled by angle and proximity.
  ///   Adds upward force and modifies Z for vertical boost.
  /// </summary>
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
    var totalVelocity = (pawnVelocity.ToVec3() + velocity) * scale;
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
      ConVarValues["ff_damage_reduction_grenade_self"] = 0f;
      ConVarValues["sv_falldamage_scale"]              = 0f;
    }

    public override float FreezeTime(CCSPlayerController player) { return 1; }
  }
}