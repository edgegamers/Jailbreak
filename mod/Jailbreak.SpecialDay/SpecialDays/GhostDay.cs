using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class GhostDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider), ISpecialDayMessageProvider {
  public static readonly FakeConVar<float> CV_VISIBLE_DURATION = new(
    "jb_sd_ghost_visible_duration",
    "Amount of time players spend visible per cycle", 5f,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<float>(1f, 30f));
  
  public static readonly FakeConVar<float> CV_INVISIBLE_DURATION = new(
    "jb_sd_ghost_invisible_duration",
    "Amount of time players spend invisible per cycle", 5f,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<float>(1f, 30f));

  public static float CycleDuration
    => CV_VISIBLE_DURATION.Value + CV_INVISIBLE_DURATION.Value;
  private bool allPlayersVisible;
  private float timeElapsed;
  private Timer? ghostTimer;
  public override SDType Type => SDType.GHOST;
  
  public virtual ISDInstanceLocale Locale
    => new SoloDayLocale("Ghost War",
      "Now you see me… now you don’t! Fight through flickering visibility!");

  public override void Setup() {
    Plugin.RegisterListener<Listeners.CheckTransmit>(OnTransmit);
    setVisibility(false);
    base.Setup();
  }

  public override void Execute() {
    base.Execute();
    
    timeElapsed = 0f;
    setVisibility(true);
    
    ghostTimer = Plugin.AddTimer(1f, () => {
      timeElapsed += 1f;
      
      var mod = timeElapsed % CycleDuration;

      var shouldBeVisible = mod < CV_VISIBLE_DURATION.Value;
      var timeLeft = (int)((shouldBeVisible ? 
        CV_VISIBLE_DURATION.Value : CycleDuration) - mod);

      if (shouldBeVisible != allPlayersVisible)
        setVisibility(shouldBeVisible);

      foreach (var player in PlayerUtil.GetAlive()
       .Where(p => p?.IsValid == true)) 
        player.PrintToCenter($"{(allPlayersVisible 
          ? "Visible" : "Hidden")} for: {timeLeft}s");
    }, TimerFlags.REPEAT);
  }

  private void OnTransmit(CCheckTransmitInfoList infolist) {
    if (allPlayersVisible) return;

    foreach (var (info, viewer) in infolist) {
      if (viewer == null || !viewer.IsValid || !viewer.PawnIsAlive) continue;
      if (viewer.TeamNum == (byte)CsTeam.Spectator) continue; // skip spec

      foreach (var target in Utilities.GetPlayers()
       .Where(target => 
          target.IsReal() && target.Slot != viewer.Slot)) {
        if (target.Pawn?.Value != null && target.Pawn.IsValid)
          info.TransmitEntities.Remove(target.Pawn.Value);
      }
    }
  }
  
  override protected HookResult OnEnd(EventRoundEnd ev, GameEventInfo info) {
    ghostTimer?.Kill();
    Server.ExecuteCommand("mp_footsteps_serverside 1");
    return base.OnEnd(ev, info);
  }

  private void setVisibility(bool state) {
    allPlayersVisible = state;
    Server.ExecuteCommand($"mp_footsteps_serverside {(state ? "1" : "0")}");
    if (state) EnableDamage(); else DisableDamage();
    foreach (var player in PlayerUtil.GetAlive()) {
      player.ExecuteClientCommand("play \"sounds/ui/counter_beep.vsnd\"");
    }
  }
}