using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Draw;

public class TextSpawner : ITextSpawner {
  public CPointWorldText CreateText(TextSetting setting, Vector position) {
    var ent = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");

    if (ent == null || !ent.IsValid)
      throw new Exception("Failed to create CPointWorldText entity");

    ent.MessageText     = setting.msg;
    ent.Enabled         = setting.enabled;
    ent.FontSize        = setting.fontSize;
    ent.Color           = setting.color;
    ent.WorldUnitsPerPx = setting.worldUnitsPerPx;
    ent.DepthOffset     = setting.depthOffset;
    ent.Fullbright      = setting.fullbright;
    ent.FontName        = setting.fontName;

    ent.Teleport(position);
    ent.DispatchSpawn();
    return ent;
  }

  public CPointWorldText CreateTextHat(TextSetting setting,
    CCSPlayerController player) {
    var position = player.PlayerPawn.Value?.AbsOrigin;
    if (position == null) throw new Exception("Failed to get player position");
    position = position.Clone();
    position.Add(new Vector(0, 72, 0));
    var ent = CreateText(setting, position);
    ent.AcceptInput("SetParent", player.PlayerPawn.Value, null, "!activator");
    return ent;
  }
}