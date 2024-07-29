using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.SpecialDay;

public abstract class AbstractSpecialDay {
  protected BasePlugin Plugin;
  public abstract SDType Type { get; }
  public abstract ISpecialDayMessages Messages { get; }
  public virtual SpecialDaySettings? Settings => null;

  private readonly Dictionary<string, object?> previousConvarValues = new();
  private readonly IServiceProvider provider;

  public AbstractSpecialDay(BasePlugin plugin, IServiceProvider provider) {
    Plugin = plugin;
    plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);
    this.provider = provider;
  }

  /// <summary>
  /// Called when the warden initially picks the special day.
  /// Use for teleporting, stripping weapons, starting timers, etc.
  /// </summary>
  public virtual void Setup() {
    if (Settings == null) return;

    foreach (var entry in Settings.ConVarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null) Server.PrintToConsole($"Invalid convar: {entry.Key}");
      previousConvarValues[entry.Key] = getConvarStringValue(cv);
      try { cv?.SetValue(entry.Value); } catch (InvalidOperationException e) {
        Console.WriteLine(e);
      }
    }

    RoundUtil.SetTimeRemaining(Settings.RoundTime());

    if (Settings.StartInvulnerable) DisableDamage();

    if (!Settings.AllowLastRequests)
      provider.GetRequiredService<ILastRequestManager>().DisableLRForRound();

    if (Settings.FreezePlayers) {
      foreach (var player in Utilities.GetPlayers()) {
        player.Freeze();
        Plugin.AddTimer(Settings.FreezeTime(player),
          () => { player.UnFreeze(); });
      }
    }

    switch (Settings.Teleport) {
      case SpecialDaySettings.TeleportType.NONE:
        break;
      case SpecialDaySettings.TeleportType.CELL:
        var spawn = Utilities
         .FindAllEntitiesByDesignerName<SpawnPoint>(
            "info_player_counterterrorist")
         .ToList();
        break;
    }
  }

  private string getConvarStringValue(ConVar? cvar) {
    try {
      if (cvar == null) return "";
      var convarValue = cvar.Type switch {
        ConVarType.Bool => cvar.GetPrimitiveValue<bool>().ToString(),
        ConVarType.Float32 or ConVarType.Float64 => cvar
         .GetPrimitiveValue<float>()
         .ToString(),
        ConVarType.UInt16 => cvar.GetPrimitiveValue<ushort>().ToString(),
        ConVarType.Int16  => cvar.GetPrimitiveValue<short>().ToString(),
        ConVarType.UInt32 => cvar.GetPrimitiveValue<uint>().ToString(),
        ConVarType.Int32  => cvar.GetPrimitiveValue<int>().ToString(),
        ConVarType.Int64  => cvar.GetPrimitiveValue<long>().ToString(),
        ConVarType.UInt64 => cvar.GetPrimitiveValue<ulong>().ToString(),
        ConVarType.String => cvar.StringValue,
        _                 => ""
      };
      return convarValue;
    } catch (Exception) { return "INVALID"; }
  }


  /// <summary>
  /// Called when the actual action begins for the special day.
  /// </summary>
  public abstract void Execute();

  [GameEventHandler]
  public virtual HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var entry in previousConvarValues) {
      var cv = ConVar.Find(entry.Key);
      if (cv == null) continue;
      try { cv.SetValue(entry.Value); } catch (InvalidOperationException e) {
        Console.WriteLine(e);
      }

      previousConvarValues.Clear();
    }

    return HookResult.Continue;
  }

  protected void DisableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) DisableDamage(player);
  }

  protected void EnableDamage() {
    foreach (var player in PlayerUtil.GetAlive()) EnableDamage(player);
  }

  protected void DisableDamage(CCSPlayerController player) {
    if (player.Pawn.Value != null) player.Pawn.Value.TakesDamage = false;
  }

  protected void EnableDamage(CCSPlayerController player) {
    if (player.Pawn.Value != null) player.Pawn.Value.TakesDamage = true;
  }
}