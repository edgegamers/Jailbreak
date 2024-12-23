using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using GangsAPI.Data;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Public.Mod.Warden;

public abstract class GenericIconBehavior(ITextSpawner spawner, Color color)
  : IPluginBehavior {
  private readonly IEnumerable<CPointWorldText>?[] icons =
    new IEnumerable<CPointWorldText>?[65];

  public void AssignIcon(CCSPlayerController player) {
    var wrapper = new PlayerWrapper(player);

    Task.Run(async () => {
      var icon = await getIcon(wrapper);

      var data = new TextSetting { msg = icon, color = color };

      await Server.NextFrameAsync(() => {
        var hat = spawner.CreateTextHat(data, player);
        icons[player.Slot] = hat;
      });
    });
  }

  public void RemoveIcon(CCSPlayerController player) {
    var hat = icons[player.Slot];
    if (hat == null) return;
    foreach (var text in hat) {
      if (!text.IsValid) continue;
      text.Remove();
    }

    icons[player.Slot] = null;
  }

  abstract protected Task<string> getIcon(PlayerWrapper player);
}