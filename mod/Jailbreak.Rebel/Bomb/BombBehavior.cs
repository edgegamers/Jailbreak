using System.Drawing;
using System.Runtime.InteropServices;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Rebel;

using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Rebel.Bomb;

public class BombBehavior : IPluginBehavior, IBombService
{
	public const string C4_NAME = "rebel_c4";
	public const float DELAY_TIME = 2.5f;
	public const float SOUND_DELAY_TIME = 1.1f;

	private ICoroutines _coroutines;
	private IRichLogService _logs;
	private IServiceProvider _provider;

	private IRebelService _rebel;
	private IJihadC4Notifications _notifications;

	private readonly MemoryFunctionVoid<CBaseEntity, string, int, float, float> _emitSound;

	public BombBehavior(ICoroutines coroutines, IRichLogService logs, IJihadC4Notifications notifications, IRebelService rebel, IServiceProvider provider)
	{
		_coroutines = coroutines;
		_logs = logs;
		_notifications = notifications;
		_rebel = rebel;
		_provider = provider;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			_emitSound = new("48 8B C4 48 89 58 10 48 89 70 18 55 57 41 56 48 8D A8 08 FF FF FF");
		else
			_emitSound = new("48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 55 41 54 49 89 FC 53 48 89 F3");
	}

	public void Start(BasePlugin parent)
	{
		parent.RegisterListener<Listeners.OnTick>(OnTick);
	}

	private void EmitSound(CBaseEntity entity, string name)
	{
		_emitSound.Invoke(entity, name, 100, 1, 0);
	}

	private void OnTick()
	{
		foreach (var player in Utilities.GetPlayers())
		{
			if (!player.IsReal() || !player.PawnIsAlive || !player.PlayerPawn.IsValid)
				continue;

			if (player.GetTeam() != CsTeam.Terrorist)
				continue;

			//	Is player holding down USE?
			if ((player.Buttons & PlayerButtons.Use) == 0)
				continue;

			//	Is the player holding a bomb out?
			var pawn = player.PlayerPawn.Value!;
			var weaponServices = pawn.WeaponServices;

			//	Sanity check
			if (weaponServices == null)
				continue;

			var weaponHandle = weaponServices.ActiveWeapon;
			if (!weaponHandle.IsValid ||weaponHandle.Value == null)
				continue;

			//	If the item has the name of the C4 bomb then detonate it.
			if (weaponHandle.Value.Globalname == C4_NAME && weaponHandle.Value.Entity.DesignerName == "weapon_c4")
				this.OnDetonate(player, new CC4(weaponHandle.Value.Handle));

		}
	}

	private void OnDetonate(CCSPlayerController bomber, CC4 bomb)
	{
		_logs.Append(_logs.Player(bomber), "triggered THE BOMB\u2122!");
		_rebel.MarkRebel(bomber);

		//	1. Bomb entity needs to be destroyed no matter what
		//	   block other players from picking it up
		//	2. bomber needs to DIE no matter what
		bomb.Globalname = "not_" + C4_NAME;
		bomb.CanBePickedUp = false;

		BombResult stats = new();
		stats.Bomber = bomber;
		stats.SteamId = bomber.AuthorizedSteamID.ToString();

		_coroutines.Round(Detonate, DELAY_TIME);
		_notifications.PlayerDetonateC4(bomber).ToAllChat().ToAllCenter();

		EmitSound(bomber, "C4.ExplodeWarning");
		_coroutines.Round(() => EmitSound(bomb, "C4.ExplodeTriggerTrip"), SOUND_DELAY_TIME);

		void ParticleSystemAt(Vector position)
		{
			CParticleSystem particleSystemEntity = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
			particleSystemEntity.EffectName = "particles/explosions_fx/explosion_c4_500.vpcf";
			particleSystemEntity.StartActive = true;

			particleSystemEntity.Teleport(position, new QAngle(), new Vector());
			particleSystemEntity.DispatchSpawn();

			_coroutines.Round(() =>
			{
				if (particleSystemEntity.IsValid)
					particleSystemEntity.Remove();
			});
		}

		void TriggerHooks()
		{
			foreach (IBombResultHook bombResultHook in _provider.GetServices<IBombResultHook>())
			{
				try
				{
					bombResultHook.OnDetonation(stats);
				}
				catch (Exception e)
				{
					Log.Warning("Error during bomb result hook", e);
				}
			}
		}

		void Detonate()
		{
			//	bomb is no longer valid?!
			if (!bomb.IsValid || bomb.AbsOrigin == null)
				return;

			var explosionOrigin = bomb.AbsOrigin;
			ParticleSystemAt(explosionOrigin);
			EmitSound(bomb, "tr.C4explode");


			foreach (var victim in Utilities.GetPlayers())
			{
				if (victim.GetTeam() != CsTeam.CounterTerrorist)
					continue;

				if (!victim.IsValid || !victim.PawnIsAlive)
					continue;

				var victimPos = victim.PlayerPawn.Value.AbsOrigin;
				float distance = explosionOrigin.Distance(victimPos);
				float damage = DamageCurves.CircleLower(500, 500, distance);
				int intDamage = (int)damage;

				_logs.Append(bomber.IsReal() ? _logs.Player(bomber) : "(the bomber)", "hurt player", _logs.Player(victim), "with THE BOMB\u2122 for", intDamage, "damage");

				stats.Damage += intDamage;
				if (intDamage == 0)
					continue;

				if (victim.PlayerPawn.Value.Health > intDamage)
				{
					victim.PlayerPawn.Value.Health = (victim.PlayerPawn.Value.Health - intDamage);
					Utilities.SetStateChanged(victim.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
				}
				else
				{
					stats.Kills += 1;
					victim.CommitSuicide(true, true);
				}
			}

			//	Lastly, always kill the bomber
			//if (bomber.IsValid && bomber.IsReal() && bomber.PawnIsAlive)
			//	bomber.CommitSuicide(true, true);

			//	Now remove the bomb
			//	bomb.Remove();

			//	Trigger hooks waiting for stats
			TriggerHooks();
		}

	}

	/// <summary>
	/// Give the player a C4
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public bool TryGrant(CCSPlayerController player)
	{
		CC4 bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));

		if (bombEntity == null)
			return false;

		bombEntity.Globalname = C4_NAME;
		bombEntity.Render = Color.Blue;

		return true;
	}

	[ConsoleCommand("css_givebomb", "Used to set the end point of a race LR")]
	[CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
	public void Command_EndRace(CCSPlayerController? executor, CommandInfo info)
	{
		this.TryGrant(executor);
	}

}
