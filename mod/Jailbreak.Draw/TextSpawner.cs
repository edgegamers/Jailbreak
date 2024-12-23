using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Draw;

public class TextSpawner : ITextSpawner {
  public CPointWorldText CreateText(TextSetting setting, Vector position,
    QAngle rot) {
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

    ent.Teleport(position, rot);
    ent.DispatchSpawn();
    return ent;
  }

  public IEnumerable<CPointWorldText> CreateTextHat(TextSetting setting,
    CCSPlayerController player) {
    var one = spawnHatPart(setting, player, 270);
    var two = spawnHatPart(setting, player, 180);

    return [one, two];
  }

  private CPointWorldText spawnHatPart(TextSetting setting,
    CCSPlayerController player, float yRot) {
    var position = player.PlayerPawn.Value?.AbsOrigin;
    var rotation = player.PlayerPawn.Value?.AbsRotation;
    if (position == null || rotation == null)
      throw new Exception("Failed to get player position");
    position = position.Clone();
    position.Add(new Vector(0, 0, 72));
    rotation = new QAngle(rotation.X, rotation.Y + yRot, rotation.Z + 90);

    // Current position is a bit to the right of the player's head
    // so we need to move it to the center
    position.Add(GetForwardVector(rotation) * -10);

    var ent = CreateText(setting, position, rotation);
    ent.AcceptInput("SetParent", player.PlayerPawn.Value, null, "!activator");
    return ent;
  }

  public static Vector GetForwardVector(QAngle rotation) {
    var forward = new Vector();
    forward.X = (float)Math.Cos(rotation.Y * Math.PI / 180);
    forward.Y = (float)Math.Sin(rotation.Y * Math.PI / 180);
    forward.Z = 0;
    return forward;
  }
}