using System.Numerics;
using CounterStrikeSharp.API.Core;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Public.Mod.Draw;

public interface ITextSpawner {
  CPointWorldText CreateText(string text, Vector position) {
    return CreateText(new TextSetting { msg = text }, position);
  }

  CPointWorldText CreateText(TextSetting setting, Vector position);

  CPointWorldText CreateTextHat(string text, CCSPlayerController player) {
    return CreateTextHat(new TextSetting { msg = text }, player);
  }

  CPointWorldText
    CreateTextHat(TextSetting setting, CCSPlayerController player);
}