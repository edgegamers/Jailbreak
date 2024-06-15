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
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Warden.Global;

// By making it a struct we ensure values from the CCSPlayerPawn are passed by VALUE.
struct PreWardenStats(int armorValue, int health, int maxHealth, bool hadHealthshot, bool hadHelmetArmor)
{
    public int armorValue = armorValue;
    public int health = health;
    public int maxHealth = maxHealth;
	public bool hadHealthshot = hadHealthshot;
	public bool hadHelmetArmor = hadHelmetArmor;
}

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
	private PreWardenStats? _preWardenStats = null;
	private bool _hasWarden;
	private CCSPlayerController? _warden;
	private int _numOfDeadGuards = 0;

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

		mute.UnPeaceMute();
		
		_hasWarden = true;
		_warden = controller;
		

		if (_warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
		    _warden.Pawn.Value.Render = Color.FromArgb(254, 0, 0, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
		}

		notifications.NEW_WARDEN(_warden)
			.ToAllChat()
			.ToAllCenter();
		
		_warden.PlayerName = "[WARDEN] " + _warden.PlayerName;
			
		foreach (var player in Utilities.GetPlayers()) {
			if (!player.IsReal()) continue;
			player.ExecuteClientCommand(
				$"play sounds/{config.WardenNewSoundName}");
		}

		logs.Append( logs.Player(_warden), "is now the warden.");

		_unblueTimer = _parent.AddTimer(3, UnmarkPrisonersBlue);
        mute.PeaceMute(hadWarden ? MuteReason.WARDEN_TAKEN : MuteReason.INITIAL_WARDEN);

        // Always store the stats of the warden b4 they became warden, in case we need to restore them later.
        CCSPlayerPawn? wardenPawn = _warden.PlayerPawn.Value;
        if(wardenPawn == null) { return false; }
		
        if (!hadWarden)
		{

            hadWarden = true;

			bool hasHealthshot = playerHasHealthshot(_warden);
			bool hasHelmet = playerHasHelmetArmor(_warden);
            _preWardenStats = new PreWardenStats(wardenPawn.ArmorValue, wardenPawn.Health, wardenPawn.MaxHealth, hasHealthshot, hasHelmet);

			if (!hasHelmet) { _warden.GiveNamedItem("item_assaultsuit"); }
            SetWardenStats(wardenPawn, 125, 125, 125);	
			if (!hasHealthshot) { _warden.GiveNamedItem("weapon_healthshot"); }

        } else 
		{
			_preWardenStats = null;
		}

		return true;
	}

    public bool TryRemoveWarden(bool isPass = false)
	{

        if (!_hasWarden)
			return false;
		
		mute.UnPeaceMute();

		_hasWarden = false;

		if (_warden != null && _warden.Pawn.Value != null)
		{
			_warden.PlayerName = _warden.PlayerName.Replace("[WARDEN] ", "");
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
			_warden.Pawn.Value.Render = Color.FromArgb(254, 255, 255, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
			logs.Append( logs.Player(_warden), "is no longer the warden.");
		}

        CCSPlayerPawn? wardenPawn = _warden!.PlayerPawn.Value;
		if (wardenPawn == null) {  return false; }

		// if isPass we restore their old health values or their current health, whichever is less.
		if (isPass && _preWardenStats != null)
		{

			// If this is true then we want to make it so the next person who claims warden receives the buff.
			if (_numOfDeadGuards == 0) 
			{
				hadWarden = false; 
			}

			// Regardless of if the above if statement is true or false, we want to restore the player's previous stats.
			SetWardenStats(wardenPawn,
				Math.Min(wardenPawn.ArmorValue, _preWardenStats.Value.armorValue),
				Math.Min(wardenPawn.Health, _preWardenStats.Value.health),
				Math.Min(wardenPawn.MaxHealth, _preWardenStats.Value.maxHealth)
			);

			/* This code makes sure people can't abuse the first warden's buff by removing it if they pass. */
			CCSPlayer_ItemServices? itemServices = itemServicesOrNull(_warden);
			if (itemServices == null) { return false; }

			if (!_preWardenStats.Value.hadHelmetArmor) { itemServices.HasHelmet = false; }
            Utilities.SetStateChanged(wardenPawn, "CBasePlayerPawn", "m_pItemServices");

            if (!_preWardenStats.Value.hadHealthshot) { playerHasHealthshot(_warden, true); }

        }
        _warden = null;
        return true;
	}

	[GameEventHandler]
	public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info)
	{
		if (ev.Userid != null && ev.Userid.Team == CsTeam.CounterTerrorist) { _numOfDeadGuards++; } 
		if(!((IWardenService)this).IsWarden(ev.Userid))
			return HookResult.Continue;
		
		mute.UnPeaceMute();
		ProcessWardenDeath();
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnChangeTeam(EventPlayerTeam @event, GameEventInfo info)
	{
		var player = @event.Userid;
		if (!((IWardenService)this).IsWarden(player))
			return HookResult.Continue;
		
		mute.UnPeaceMute();
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
		if (specialTreatment.IsSpecialTreatment(player))
			return true;
		if (rebels.IsRebel(player))
			return true;
		return false;
	}

    private int getBalance()
    {
        var ctCount = Utilities.GetPlayers().Count(p => p.Team == CsTeam.CounterTerrorist);
        var tCount = Utilities.GetPlayers().Count(p => p.Team == CsTeam.Terrorist);

		// Casting to a float ensures if we're diving by zero, we get infinity instead of an error.
        var ratio = ((float) tCount / config.TerroristRatio) - ctCount;

        return ratio switch
        {
            > 0 => 1,
            0 => 0,
            _ => -1
        };
    }

	private CCSPlayer_ItemServices? itemServicesOrNull(CCSPlayerController player)
	{
		CPlayer_ItemServices? itemServices = player.PlayerPawn.Value?.ItemServices;
		return (itemServices != null) ? new CCSPlayer_ItemServices(itemServices.Handle) : null;
    }

	private void SetWardenStats(CCSPlayerPawn wardenPawn, int armorValue, int health, int maxHealth)
	{
        wardenPawn.ArmorValue = armorValue;
        wardenPawn.Health = health;
        wardenPawn.MaxHealth = maxHealth;

        Utilities.SetStateChanged(wardenPawn, "CCSPlayerPawn", "m_ArmorValue");
        Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iHealth");
        Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iMaxHealth");
    }

	private bool playerHasHelmetArmor(CCSPlayerController player)
	{
		CCSPlayer_ItemServices? itemServices = itemServicesOrNull(player);
		return (itemServices != null) ? itemServices.HasHelmet : false;
    }

	private bool playerHasHealthshot(CCSPlayerController player, bool removeIfHas = false)
	{
		CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;
		if (playerPawn == null || playerPawn.WeaponServices == null) { return false; }

        foreach (var weapon in playerPawn.WeaponServices.MyWeapons)
        {
            if (weapon.Value == null) continue;
            if (weapon.Value.DesignerName.Equals("weapon_healthshot")) 
			{ 
				if (removeIfHas) { weapon.Value.Remove(); }
				return true; 
			}
        }
		return false;
    }

    [GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
	{
		this.TryRemoveWarden();
		mute.UnPeaceMute();
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{

		hadWarden = false;
		_numOfDeadGuards = 0;
		_preWardenStats = null;

        int ctArmorValue = getBalance() switch
        {
            0 => 50, // Balanced teams
            1 => 100, // Ts outnumber CTs
            -1 => 25, // CTs outnumber Ts
            _ => 50 // default (should never happen)
        };

		/* Round start CT buff */
		foreach (var guardController in Utilities.GetPlayers().Where(p => p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
		{

			CCSPlayerPawn? guardPawn = guardController.PlayerPawn.Value;
			if (guardPawn == null) {  continue; }

			guardPawn.ArmorValue = ctArmorValue;
			Utilities.SetStateChanged(guardPawn, "CCSPlayerPawn", "m_ArmorValue");

        }

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
