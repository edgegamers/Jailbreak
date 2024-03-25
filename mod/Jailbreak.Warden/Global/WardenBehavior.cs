using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{
	private ILogger<WardenBehavior> _logger;
	private IRichLogService _logs;
	private IWardenNotifications _notifications;
	private ISpecialTreatmentService _specialTreatment;
	private IRebelService _rebels;
	private ISet<CCSPlayerController> _bluePrisoners = new HashSet<CCSPlayerController>();
	private BasePlugin _parent;

	private bool _hasWarden;
	private CCSPlayerController? _warden;

	public WardenBehavior(ILogger<WardenBehavior> logger, IWardenNotifications notifications, IRichLogService logs,
		ISpecialTreatmentService specialTreatment, IRebelService rebels)
	{
		_logger = logger;
		_notifications = notifications;
		_logs = logs;
		_specialTreatment = specialTreatment;
		_rebels = rebels;
	}

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

		if (_warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
		    _warden.Pawn.Value.Render = Color.FromArgb(254, 0, 0, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
		}

		_notifications.NEW_WARDEN(_warden)
			.ToAllChat()
			.ToAllCenter();

		_logs.Append( _logs.Player(_warden), "is now the warden.");

		_parent.AddTimer(3, UnmarkPrisonersBlue);
		return true;
	}

	public bool TryRemoveWarden()
	{
		if (!_hasWarden)
			return false;

		_hasWarden = false;

		if (_warden != null && _warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
			_warden.Pawn.Value.Render = Color.FromArgb(254, 255, 255, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
			_logs.Append( _logs.Player(_warden), "is no longer the warden.");
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
			_logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

		//	Warden died!
		_notifications.WARDEN_DIED
			.ToAllChat()
			.ToAllCenter();

		_notifications.BECOME_NEXT_WARDEN.ToAllChat();
		
		MarkPrisonersBlue();
	}
	
	private void UnmarkPrisonersBlue()
	{
		foreach (var player in _bluePrisoners)
		{
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
		foreach(CCSPlayerController player in Utilities.GetPlayers())
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
		if (_specialTreatment.IsSpecialTreatment(player))
			return true;
		if (_rebels.IsRebel(player))
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
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
	{
		if(!((IWardenService) this).IsWarden(ev.Userid))
			return HookResult.Continue;
		
		if (!this.TryRemoveWarden())
			_logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


		_notifications.WARDEN_LEFT
			.ToAllChat()
			.ToAllCenter();

		_notifications.BECOME_NEXT_WARDEN.ToAllChat();

		return HookResult.Continue;
	}
}
