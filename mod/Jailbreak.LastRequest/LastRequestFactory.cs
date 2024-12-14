using System;
using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.LastRequest.LastRequests;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public class LastRequestFactory(ILastRequestManager manager,
  IServiceProvider services) : ILastRequestFactory {
  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) { plugin = basePlugin; }

  public AbstractLastRequest CreateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type) {
    return type switch {
      LRType.KNIFE_FIGHT => new KnifeFight(plugin, services, prisoner, guard),
      LRType.GUN_TOSS    => new GunToss(plugin, manager, prisoner, guard),
      LRType.NO_SCOPE    => new NoScope(plugin, services, prisoner, guard),
      LRType.SHOT_FOR_SHOT => new BulletForBullet(plugin, services, prisoner,
        guard, false),
      LRType.ROCK_PAPER_SCISSORS => new RockPaperScissors(plugin, services,
        prisoner, guard),
      LRType.COINFLIP => new Coinflip(plugin, services, prisoner, guard),
      LRType.RACE => new Race(plugin, manager, prisoner, guard,
        services.GetRequiredService<ILRRaceLocale>()),
      LRType.MAG_FOR_MAG => new BulletForBullet(plugin, services, prisoner,
        guard, true),
      _ => throw new ArgumentException("Invalid last request type: " + type,
        nameof(type))
    };
  }

  public bool IsValidType(LRType type) {
    return type switch {
      LRType.KNIFE_FIGHT         => true,
      LRType.GUN_TOSS            => true,
      LRType.NO_SCOPE            => true,
      LRType.SHOT_FOR_SHOT       => true,
      LRType.ROCK_PAPER_SCISSORS => true,
      LRType.COINFLIP            => true,
      LRType.RACE                => true,
      LRType.MAG_FOR_MAG         => true,
      _                          => false
    };
  }
}