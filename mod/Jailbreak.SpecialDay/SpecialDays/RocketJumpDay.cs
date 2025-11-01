using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
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

  private readonly MemoryFunctionVoid<CBaseGrenade, CBaseEntity>
    bounce = new("48 83 BE ? ? ? ? ? 74");

  private readonly HashSet<CCSPlayerPawn> jumping = [];
  private Dictionary<ulong, float> nextNova = new();

  public override SDType Type => SDType.ROCKETJUMP;
  public override SpecialDaySettings Settings => new RocketJumpSettings();

  public ISDInstanceLocale Locale
    => new SoloDayLocale("Rocket Jump",
      "Your shotgun is now an RPG that fires grenades! "
      + "Shoot the ground to launch! " + "Mid-air knives Insta-kill!");

  public override void Setup() {
    Plugin.HookUserMessage(GE_FIRE_BULLETS_ID, fireBulletsUmHook);
    bounce.Hook(CBaseGrenade_Bounce, HookMode.Pre);
    Plugin.RegisterEventHandler<EventWeaponFire>(onWeaponFire);
    VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(onHurt, HookMode.Pre);
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
    bounce.Unhook(CBaseGrenade_Bounce, HookMode.Pre);
    Plugin.DeregisterEventHandler<EventWeaponFire>(onWeaponFire);
    VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(onHurt, HookMode.Pre);
    Plugin.RemoveListener<Listeners.OnTick>(onTick);

    // Delay to avoid mutation during hook execution
    Server.NextFrameAsync(() => { jumping.Clear(); });

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
  private HookResult CBaseGrenade_Bounce(DynamicHook hook) {
    var projectile = hook.GetParam<CBaseGrenade>(0);
    if (projectile.DesignerName != "hegrenade_projectile")
      return HookResult.Continue;

    var owner = projectile.OwnerEntity.Value?.As<CCSPlayerPawn>();
    if (owner == null || owner.DesignerName != "player")
      return HookResult.Continue;

    var bulletOrigin = projectile.AbsOrigin;
    var pawnOrigin   = owner.AbsOrigin;
    if (bulletOrigin == null || pawnOrigin == null) return HookResult.Continue;

    var eyeOrigin = owner.GetEyeOrigin();
    var distance = Vector3.Distance(bulletOrigin.ToVec3(), pawnOrigin.ToVec3());

    projectile.DetonateTime  = 0f;
    projectile.NextThinkTick = Server.TickCount + 1;
    doJump(owner, distance, bulletOrigin.ToVec3(), eyeOrigin);

    return HookResult.Handled;
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

    var pawn   = controller.PlayerPawn.Value;
    var origin = pawn?.AbsOrigin;
    if (pawn == null || origin == null) return HookResult.Continue;
    pawn.GetEyeForward(10.0f, out var forwardDir, out var targetPos);

    var realBulletVelocity = forwardDir * CV_BULLET_SPEED.Value;
    var addedBulletVelocity = CV_PROJ_INHERIT_PLAYER_VELOCITY.Value ?
      pawn.AbsVelocity.ToVec3() + realBulletVelocity :
      realBulletVelocity;
    shootBullet(controller, targetPos, addedBulletVelocity,
      new Vector3(origin.X, origin.Y, (float)(origin.Z + 64.09)));

    return HookResult.Continue;
  }

  /// <summary>
  ///   Makes knife hits lethal only if the attacker is airborne via rocket jump.
  ///   Nullifies Nova Pellet Damage
  ///   Passes Grenades Per Usual
  /// </summary>
  private HookResult onHurt(DynamicHook hook) {
    var info       = hook.GetParam<CTakeDamageInfo>(1);
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
    Vector3 velocity, Vector3 angle) {
    var pawn = controller.PlayerPawn.Value;
    if (pawn == null) return;

    var projectile =
      Utilities
       .CreateEntityByName<CHEGrenadeProjectile>("hegrenade_projectile");
    if (projectile == null) return;

    projectile.OwnerEntity.Raw = pawn.EntityHandle.Raw;
    projectile.Damage          = CV_PROJ_DAMAGE.Value;
    projectile.DmgRadius       = CV_PROJ_DAMAGE_RADIUS.Value;
    projectile.DispatchSpawn();
    projectile.AcceptInput("InitializeSpawnFromWorld", pawn, pawn);
    Schema.SetSchemaValue(projectile.Handle, "CBaseGrenade", "m_hThrower",
      pawn.EntityHandle.Raw);
    projectile.GravityScale = CV_PROJ_GRAVITY.Value;
    projectile.DetonateTime = 9999f;

    // Set transform BY VALUE (no unsafe pointers)
    var pos = new Vector(origin.X, origin.Y, origin.Z);
    var vel = new Vector(velocity.X, velocity.Y, velocity.Z);
    var ang = new QAngle(angle.X, angle.Y, angle.Z);

    projectile.Teleport(pos, ang, vel);
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