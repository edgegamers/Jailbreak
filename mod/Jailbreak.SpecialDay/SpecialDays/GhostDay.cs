using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class GhostDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider), ISpecialDayMessageProvider {
  public static readonly FakeConVar<float> CV_VISIBLE_DURATION = new(
    "css_jb_sd_ghost_visible_duration",
    "Amount of time players spend visible per cycle", 5f,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<float>(1f, 30f));
  
  public static readonly FakeConVar<float> CV_INVISIBLE_DURATION = new(
    "css_jb_sd_ghost_invisible_duration",
    "Amount of time players spend invisible per cycle", 5f,
    ConVarFlags.FCVAR_NONE, new NonZeroRangeValidator<float>(1f, 30f));

  private static readonly
    MemoryFunctionVoid<nint, nint, int, nint, nint, nint, int, bool>
    CHECK_TRANSMIT = new(GameData.GetSignature("CheckTransmit"));

  private static readonly int CHECK_TRANSMIT_PLAYER_SLOT_CACHE =
    GameData.GetOffset("CheckTransmitPlayerSlot");

  private static float CycleDuration
    => CV_VISIBLE_DURATION.Value + CV_INVISIBLE_DURATION.Value;
  private bool allPlayersVisible;
  private float timeElapsed;
  private Timer? ghostTimer;
  
  [StructLayout(LayoutKind.Sequential)]
  public struct CCheckTransmitInfo
  {
    public CFixedBitVecBase m_pTransmitEntity;
  }

  [StructLayout(LayoutKind.Sequential)]
  public readonly unsafe struct CFixedBitVecBase
  {
    private const int LOG2_BITS_PER_INT = 5;
    private const int MAX_EDICT_BITS = 14;
    private const int BITS_PER_INT = 32;
    private const int MAX_EDICTS = 1 << MAX_EDICT_BITS;

    private readonly uint* m_Ints;

    public void Clear(int bitNum)
    {
      if (bitNum is < 0 or >= MAX_EDICTS)
        return;

      var pInt = m_Ints + (bitNum >> LOG2_BITS_PER_INT);
      *pInt &= ~(uint)(1 << (bitNum & BITS_PER_INT - 1));
    }
  }
  public override SDType Type => SDType.GHOST;
  public override ISDInstanceLocale Locale
    => new SoloDayLocale("Ghost War",
      "Now you see me… now you don’t! Fight through flickering visibility!");
  public override SpecialDaySettings Settings => new GhostSettings();

  public override void Setup() {
    CHECK_TRANSMIT.Hook(onTransmit, HookMode.Post);
    Server.NextFrameAsync(() => { setVisibility(false); });
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
       .Where(p => p.IsValid)) 
        player.PrintToCenter($"{(allPlayersVisible 
          ? "Visible" : "Hidden")} for: {timeLeft}s");
    }, TimerFlags.REPEAT);
  }

  private unsafe HookResult onTransmit(DynamicHook hook) {
    if (allPlayersVisible) return HookResult.Continue;

    var ppInfoList = (nint*)hook.GetParam<nint>(1);
    var infoCount  = hook.GetParam<int>(2);

    var players = Utilities.GetPlayers()
     .Where(p => p is { IsValid: true, PawnIsAlive: true })
     .ToList();

    for (var i = 0; i < infoCount; i++) {
      var pInfo  = ppInfoList[i];
      var slot   = *(byte*)(pInfo + CHECK_TRANSMIT_PLAYER_SLOT_CACHE);
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) continue;

      var info = Marshal.PtrToStructure<CCheckTransmitInfo>(pInfo);

      foreach (var target in
        players.Where(target => target.Index != player.Index)) {
        info.m_pTransmitEntity.Clear((int)target.PlayerPawn.Value.Index);
      }
    }
    return HookResult.Continue;
  }
  
  override protected HookResult OnEnd(EventRoundEnd ev, GameEventInfo info) {
    ghostTimer?.Kill();
    CHECK_TRANSMIT.Unhook(onTransmit, HookMode.Post);
    return base.OnEnd(ev, info);
  }

  private void setVisibility(bool state) {
    allPlayersVisible = state;
    Server.ExecuteCommand($"mp_footsteps_serverside {(state ? "1" : "0")}");
    if (state) EnableDamage(); else DisableDamage();
    foreach (var player in PlayerUtil.GetAlive()) {
      player.ExecuteClientCommand(
        $"play {(state ? "\"sounds/buttons/bell1.vsnd\"" : "\"sounds/ui/counter_beep.vsnd\"")}");
    }
  }

  public class GhostSettings : FFASettings {
    public GhostSettings() {
      ConVarValues["mp_footsteps_serverside"] = false;
    }
  }
}