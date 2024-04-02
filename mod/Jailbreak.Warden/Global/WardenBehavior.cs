using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Warden.Global;

public class WardenBehavior(
	ILogger<WardenBehavior> logger,
	IWardenNotifications notifications,
	IRichLogService logs,
	ISpecialTreatmentService specialTreatment,
	IRebelService rebels,
	WardenConfig config,
	IMuteService mute)
	: IPluginBehavior, IWardenService
{
	private ISet<CCSPlayerController> _bluePrisoners = new HashSet<CCSPlayerController>();
	private BasePlugin _parent;
	private Timer? _unblueTimer;
	private bool hadWarden = false;
	private bool firstWarden = false;

	private bool _hasWarden;
	private CCSPlayerController? _warden;

	public void Start(BasePlugin parent)
	{
		_parent = parent;
	}

	/// <summary>
	/// Get the current warden, if there is one.
	/// </summary>
	public CCSPlayerController? Warden => _warden;

	/// <summary>
	/// Whether or not a warden is currently assigned
	/// </summary>
	public bool HasWarden => _hasWarden;

	public bool TrySetWarden(CCSPlayerController controller)
	{
		if (_hasWarden)
			return false;

		//	Verify player is a CT
		if (controller.GetTeam() != CsTeam.CounterTerrorist)
			return false;
		if (!controller.PawnIsAlive)
			return false;

		_hasWarden = true;
		_warden = controller;
		
		_warden.PlayerName = "[WARDEN] " + _warden.PlayerName;

		if (_warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
		    _warden.Pawn.Value.Render = Color.FromArgb(254, 0, 0, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
		}

		notifications.NEW_WARDEN(_warden)
			.ToAllChat()
			.ToAllCenter();
			
		foreach (var player in Utilities.GetPlayers()) {
			if (!player.IsReal()) continue;
			player.ExecuteClientCommand(
				$"play sounds/{config.WardenNewSoundName}");
		}

		logs.Append( logs.Player(_warden), "is now the warden.");

		_unblueTimer = _parent.AddTimer(3, UnmarkPrisonersBlue);
		
		mute.PeaceMute(hadWarden ? MuteReason.WARDEN_TAKEN : MuteReason.INITIAL_WARDEN);

		if (!hadWarden)
		{ // First warden gets boost
			_warden.SetHp(125);
			_warden.PawnArmor = 125;
			firstWarden = true;
		}
		else
		{ // Already had a warden, update flag
			firstWarden = false;
		}
		
		hadWarden = true;
		return true;
	}

	public bool TryRemoveWarden()
	{
		if (!_hasWarden)
			return false;

		_hasWarden = false;

		if (_warden != null && _warden.Pawn.Value != null)
		{
			_warden.PlayerName = _warden.PlayerName.Replace("[WARDEN] ", "");
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
			_warden.Pawn.Value.Render = Color.FromArgb(254, 255, 255, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
			logs.Append( logs.Player(_warden), "is no longer the warden.");

			if (_warden.PawnIsAlive && firstWarden)
			{
				_warden.SetHp(Math.Min(100, (int) _warden.PawnHealth));
				_warden.PawnArmor = Math.Min(100, _warden.PawnArmor);
			}
		}

		_warden = null;

		return true;
	}

	[GameEventHandler]
	public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info)
	{
		if(!((IWardenService)this).IsWarden(ev.Userid))
			return HookResult.Continue;
		
		ProcessWardenDeath();
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnChangeTeam(EventPlayerTeam @event, GameEventInfo info)
	{
		var player = @event.Userid;
		if (!((IWardenService)this).IsWarden(player))
			return HookResult.Continue;
		
		ProcessWardenDeath();	
		return HookResult.Continue;
	}

	private void ProcessWardenDeath()
	{
		if (!this.TryRemoveWarden())
			logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

		//	Warden died!
		notifications.WARDEN_DIED
			.ToAllChat()
			.ToAllCenter();
		
		foreach (var player in Utilities.GetPlayers()) {
			if (!player.IsReal()) continue;
			player.ExecuteClientCommand(
				$"play sounds/{config.WardenKilledSoundName}");
		}

		notifications.BECOME_NEXT_WARDEN.ToAllChat();

		_unblueTimer?.Kill(); // If the warden dies withing 3 seconds of becoming warden, we need to cancel the unblue timer
		MarkPrisonersBlue();
	}
	
	private void UnmarkPrisonersBlue()
	{
		foreach (var player in _bluePrisoners)
		{
			if(!player.IsReal())
				continue;
			var pawn = player.Pawn.Value;
			if (pawn == null)
				continue;
			if(IgnoreColor(player))
				continue;
			pawn.RenderMode = RenderMode_t.kRenderNormal;
			pawn.Render = Color.FromArgb(254, 255, 255, 255);
			Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
		}
		_bluePrisoners.Clear();
	}

	private void MarkPrisonersBlue()
	{
		foreach(var player in Utilities.GetPlayers())
		{
			if(!player.IsReal() || player.Team != CsTeam.Terrorist)
				continue;
			if(IgnoreColor(player))
				continue;

			var pawn = player.Pawn.Value;
			if(pawn == null)
				continue;
			pawn.RenderMode = RenderMode_t.kRenderTransColor;
			pawn.Render = Color.FromArgb(254, 0, 0, 255);
			Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

			_bluePrisoners.Add(player);
		}
	}

	private bool IgnoreColor(CCSPlayerController player)
	{
		if (specialTreatment.IsSpecialTreatment(player))
			return true;
		if (rebels.IsRebel(player))
			return true;
		return false;
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
	{
		this.TryRemoveWarden();

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{
		hadWarden = false;
		firstWarden = false;
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
	{
		if(!((IWardenService) this).IsWarden(ev.Userid))
			return HookResult.Continue;
		
		if (!this.TryRemoveWarden())
			logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


		notifications.WARDEN_LEFT
			.ToAllChat()
			.ToAllCenter();

		foreach (var player in Utilities.GetPlayers()) {
			if (!player.IsReal()) continue;
			player.ExecuteClientCommand(
				$"play sounds/{config.WardenPassedSoundName}");
		}

		notifications.BECOME_NEXT_WARDEN.ToAllChat();

		return HookResult.Continue;
	}
}
