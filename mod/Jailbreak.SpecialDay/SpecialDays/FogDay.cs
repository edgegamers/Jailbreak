using System.Drawing;
using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FogDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.FOG;
  private FogDayLocale Msg => (FogDayLocale)Locale;
  public ISDInstanceLocale Locale => new FogDayLocale();
  public override SpecialDaySettings Settings => new FogSettings();

  private CFogController? fogController;
  private nint originalSkyMat;
  private nint originalSkyMatLighting;
  private CEnvSky? skyEntity;

  private float targetFogEnd = 2000f;
  private float fogStepPerTick;
  private bool shouldGrowFog;

  public override void Setup() {
    Plugin.RegisterListener<Listeners.OnTick>(onTick);
    Plugin.RegisterEventHandler<EventPlayerDeath>(onDeath);

    setFog(true);
    setSky(true);

    Timers[20] += () => Locale.BeginsIn(20).ToAllChat();
    Timers[25] += () => {
      Msg.FogComingIn().ToAllChat();
      setTargetFogDistance(300, 10);
    };
    Timers[30] += () => Msg.BeginsIn(10).ToAllChat();
    Timers[35] += () => Msg.BeginsIn(5).ToAllChat();
    Timers[40] += () => {
      Execute();
      Msg.BeginsIn(0);
    };
    Timers[97] += () => Msg.FogExpandsIn(3);
    Timers[98] += () => Msg.FogExpandsIn(2);
    Timers[99] += () => Msg.FogExpandsIn(1);
    Timers[100] += () => {
      setTargetFogDistance(3000, 180);
      Msg.FogExpandsIn(0);
    };

    base.Setup();
  }


  private unsafe void setSky(bool set) {
    cacheOriginalSky();

    if (skyEntity == null) return;

    var mat    = set ? nint.Zero : originalSkyMat;
    var matLit = set ? nint.Zero : originalSkyMatLighting;

    Unsafe.Write((void*)skyEntity.SkyMaterial.Handle, mat);
    Unsafe.Write((void*)skyEntity.SkyMaterialLightingOnly.Handle, matLit);

    Utilities.SetStateChanged(skyEntity, "CEnvSky", "m_hSkyMaterial");
    Utilities.SetStateChanged(skyEntity, "CEnvSky",
      "m_hSkyMaterialLightingOnly");

    skyEntity.BrightnessScale = 1.0f;
    Utilities.SetStateChanged(skyEntity, "CEnvSky", "m_flBrightnessScale");
  }

  private void setFog(bool set) {
    if (!set) {
      fogController?.Remove();
      fogController = null;
      return;
    }

    fogController ??=
      Utilities.CreateEntityByName<CFogController>("env_fog_controller");
    fogController?.DispatchSpawn();

    if (fogController == null) return;

    var fog = fogController.Fog;
    fog.Enable       = true;
    fog.Blend        = true;
    fog.ColorPrimary = Color.Black;
    fog.Exponent     = 0.1f;
    fog.Maxdensity   = 1f;
    fog.Start        = 0;
    fog.End          = 300;
    fog.Farz         = 310;

    foreach (var field in new[] {
      "colorPrimary", "start", "end", "farz", "maxdensity", "exponent",
      "enable", "blend",
    }) {
      Utilities.SetStateChanged(fogController, "CFogController", "m_fog",
        Schema.GetSchemaOffset("fogparams_t", field));
    }

    var vis = Utilities
     .FindAllEntitiesByDesignerName<CPlayerVisibility>("env_player_visibility")
     .FirstOrDefault();
    if (vis != null) {
      vis.FogMaxDensityMultiplier = 1f;
      Utilities.SetStateChanged(vis, "CPlayerVisibility",
        "m_flFogMaxDensityMultiplier");
    }

    foreach (var pawn in Utilities.GetPlayers()
     .Select(p => p.Pawn.Value)
     .OfType<CBasePlayerPawn>())
      pawn.AcceptInput("SetFogController", activator: fogController,
        value: "!activator");
  }

  private void onTick() {
    if (!shouldGrowFog || fogController == null) return;

    var fog = fogController.Fog;

    if (Math.Abs(fog.End - targetFogEnd) < Math.Abs(fogStepPerTick)) {
      fog.End       = targetFogEnd;
      fog.Farz      = targetFogEnd * 1.04f;
      shouldGrowFog = false;
    } else {
      fog.End  += fogStepPerTick;
      fog.Farz =  fog.End + 10f;
    }

    Utilities.SetStateChanged(fogController, "CFogController", "m_fog",
      Schema.GetSchemaOffset("fogparams_t", "end"));
    Utilities.SetStateChanged(fogController, "CFogController", "m_fog",
      Schema.GetSchemaOffset("fogparams_t", "farz"));
  }

  private HookResult onDeath(EventPlayerDeath @event, GameEventInfo info) {
    var pawn = @event.Userid?.PlayerPawn.Value;
    if (pawn == null) return HookResult.Continue;

    Server.NextFrame(() => {
      pawn.AcceptInput("SetFogController", activator: fogController,
        value: "!activator");
    });
    return HookResult.Continue;
  }

  override protected HookResult OnEnd(EventRoundEnd @event,
    GameEventInfo info) {
    Plugin.RemoveListener<Listeners.OnTick>(onTick);
    Plugin.DeregisterEventHandler<EventPlayerDeath>(onDeath);
    return base.OnEnd(@event, info);
  }

  private unsafe void cacheOriginalSky() {
    skyEntity = Utilities.FindAllEntitiesByDesignerName<CEnvSky>("env_sky")
     .FirstOrDefault();
    if (skyEntity == null) return;

    originalSkyMat = Unsafe.Read<nint>((void*)skyEntity.SkyMaterial.Handle);
    originalSkyMatLighting =
      Unsafe.Read<nint>((void*)skyEntity.SkyMaterialLightingOnly.Handle);
  }

  private void setTargetFogDistance(int distance, float durationInSeconds) {
    if (fogController == null) return;

    var currentEnd = fogController.Fog.End;
    var totalTicks = durationInSeconds * 64f;
    fogStepPerTick = (distance - currentEnd) / totalTicks;
    targetFogEnd   = distance;
    shouldGrowFog  = true;
  }

  public class FogSettings : SpecialDaySettings {
    private readonly Random rng;

    public FogSettings() {
      TTeleport    = TeleportType.ARMORY;
      CtTeleport   = TeleportType.ARMORY;
      OpenCells    = true;
      StripToKnife = false;
      rng          = new Random();
      WithFriendlyFire();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return rng.NextSingle() * 5 + 2;
    }
  }
}