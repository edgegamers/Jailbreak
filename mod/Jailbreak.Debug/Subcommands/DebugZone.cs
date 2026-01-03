using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_zone [add/set/remove/tpto] [type]
// css_zone draw <type>
public class DebugZone(IServiceProvider services, BasePlugin plugin)
  : AbstractCommand(services) {
  private readonly IDictionary<ulong, ITypedZoneCreator> creators =
    new Dictionary<ulong, ITypedZoneCreator>();

  private readonly IZoneFactory factory =
    services.GetRequiredService<IZoneFactory>();

  private readonly IZoneManager zoneManager =
    services.GetRequiredService<IZoneManager>();

  private readonly IBeamShapeFactory shapeFactory =
    services.GetRequiredService<IBeamShapeFactory>();

  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (executor == null) return;
    if (executor.PlayerPawn.Value?.AbsOrigin == null) {
      info.ReplyToCommand(
        "Unable to find your position. Please try again later.");
      return;
    }

    var position = executor.PlayerPawn.Value.AbsOrigin;

    if (info.ArgCount <= 1) {
      sendUsage(executor);
      return;
    }

    ZoneType? specifiedType = null;

    if (info.ArgCount == 3) {
      if (!Enum.TryParse(info.GetArg(2), true, out ZoneType success)) {
        info.ReplyToCommand("Invalid zone type");
        var typeNames = Enum.GetValues<ZoneType>().Select(s => s.ToString());
        info.ReplyToCommand("Valid types: " + string.Join(", ", typeNames));
        return;
      }

      specifiedType = success;
    }

    var map = Server.MapName;
    switch (info.GetArg(1).ToLower()) {
      case "finish":
      case "done":
        if (!creators.TryGetValue(executor.SteamID, out var creator)) {
          info.ReplyToCommand("No zone creation in progress");
          return;
        }

        var zone = creator.Build(factory);
        executor.PrintToChat(
          $"Zone created. Area: {zone.GetArea()} Center: {zone.CalculateCenterPoint()}");
        executor.PrintToChat("Pushing zone...");
        Task.Run(async () => {
          await zoneManager.PushZone(zone, creator.Type);
          await Server.NextFrameAsync(() => {
            executor.PrintToChat($"Successfully created {creator.Type} zone");
            creator.Dispose();
            creators.Remove(executor.SteamID);
          });
        });

        return;
      case "show":
      case "draw":
      case "display":
        var zoneCount = 0;
        foreach (var type in Enum.GetValues<ZoneType>()) {
          if (specifiedType != null && type != specifiedType) continue;
          var displayZones = zoneManager.GetZones(map, type)
           .GetAwaiter()
           .GetResult();
          foreach (var z in displayZones)
            z.Draw(plugin, shapeFactory, type.GetColor(), 120);

          zoneCount += displayZones.Count;
        }

        if (zoneCount == 0) {
          info.ReplyToCommand(specifiedType.HasValue ?
            $"No {specifiedType} zones found" :
            "No zones found");
          return;
        }

        info.ReplyToCommand(specifiedType.HasValue ?
          $"Showing {zoneCount} {specifiedType} zones" :
          $"Showing {zoneCount} zones");
        return;
      case "remove":
      case "delete":
        var toDelete = getUniqueZone(executor, specifiedType);
        if (toDelete == null) return;
        Task.Run(async () => {
          await zoneManager.DeleteZone(toDelete.Value.Item1.Id, map);
          await Server.NextFrameAsync(() => {
            executor.PrintToChat("Deleted zone #" + toDelete.Value.Item1.Id);
          });
        });

        return;
      case "addinner":
        var innerPair = getUniqueZone(executor, specifiedType);
        if (innerPair == null) return;
        var innerZone = innerPair.Value.Item1;
        innerZone.AddPoint(position);
        Task.Run(async () => {
          await zoneManager.DeleteZone(innerZone.Id, map);
          await zoneManager.PushZoneWithID(innerZone, innerPair.Value.Item2,
            map);
          await Server.NextFrameAsync(() => {
            info.ReplyToCommand("Added point to zone #" + innerZone.Id);
          });
        });
        return;
      case "reload":
      case "refresh":
        Task.Run(async () => {
          await zoneManager.LoadZones(Server.MapName);
          var count = (await zoneManager.GetAllZones()).SelectMany(e => e.Value)
           .Count();
          await Server.NextFrameAsync(() => {
            executor.PrintToChat($"Reloaded {count} zones");
          });
        });

        return;
      case "list":
      case "ls":
        var allZones = zoneManager.GetAllZones().GetAwaiter().GetResult();
        if (allZones.Count == 0) {
          info.ReplyToCommand("No zones found");
          return;
        }

        if (specifiedType == null) {
          foreach (var type in Enum.GetValues<ZoneType>()) {
            var zones = zoneManager.GetZones(Server.MapName, type)
             .GetAwaiter()
             .GetResult();
            if (!allZones.ContainsKey(type)) continue;

            info.ReplyToCommand($"{type} zones: {zones.Count}");
          }

          return;
        }

        var toList = zoneManager.GetZones(map, specifiedType.Value)
         .GetAwaiter()
         .GetResult();
        foreach (var listZone in toList)
          info.ReplyToCommand(
            $"#{listZone.Id} Points: {listZone.GetBorderPoints().Count()}/{listZone.GetAllPoints().Count()} Center: {listZone.CalculateCenterPoint()} Area: {listZone.GetArea()}");
        return;
      case "cleanup":
        // Cleanup auto-generated zones
        // Remove spawns that are inside of any DO NOT TELEPORT zones
        var spawns = zoneManager.GetZones(map, ZoneType.SPAWN_AUTO)
         .GetAwaiter()
         .GetResult();
        if (spawns.Count == 0) {
          info.ReplyToCommand("No auto-generated zones found");
          return;
        }

        var doNotTeleport = zoneManager
         .GetZones(Server.MapName,
            ZoneTypeExtensions.DoNotTeleports().ToArray())
         .GetAwaiter()
         .GetResult();

        var toRemove = spawns.Where(spawn
            => doNotTeleport.Any(d
              => d.IsInsideZone(spawn.CalculateCenterPoint())))
         .ToList();

        info.ReplyToCommand("Removing " + toRemove.Count
          + " auto-generated zones");
        Task.Run(async () => {
          foreach (var z in toRemove) await zoneManager.DeleteZone(z.Id, map);
        });
        return;
    }

    if (info.ArgCount != 3) {
      sendUsage(executor);
      return;
    }

    System.Diagnostics.Debug.Assert(specifiedType != null,
      nameof(specifiedType) + " != null");
    switch (info.GetArg(1).ToLower()) {
      case "add":
        attemptBeginCreation(executor, specifiedType.Value);
        return;
      case "set":
        var zones = zoneManager.GetZones(Server.MapName, specifiedType.Value)
         .GetAwaiter()
         .GetResult();

        // Server.NextFrameAsync(async () => {
        Task.Run(async () => {
          var copy = zones.ToList();

          foreach (var zone in copy)
            await zoneManager.DeleteZone(zone.Id, Server.MapName);

          await Server.NextFrameAsync(()
            => attemptBeginCreation(executor, specifiedType.Value));
        });

        return;
      case "tpto":
      case "tp":
        var tpDestinations = zoneManager.GetAllZones()
         .GetAwaiter()
         .GetResult()
         .Where(z => specifiedType == z.Key)
         .SelectMany(s => s.Value)
         .ToList();

        if (tpDestinations.Count == 0) {
          info.ReplyToCommand("No zones found");
          return;
        }

        tpDestinations.Sort((a, b)
          => a.GetMinDistanceSquared(position)
          <= b.GetMinDistanceSquared(position) ?
            -1 :
            1);
        executor.PlayerPawn.Value.Teleport(tpDestinations.First()
         .GetCenterPoint());

        info.ReplyToCommand("Teleported to zone #" + tpDestinations.First().Id);
        return;
      case "generate":
        if (specifiedType != ZoneType.ARMORY
          && specifiedType != ZoneType.CELL) {
          info.ReplyToCommand("Invalid zone type");
          return;
        }

        var entName = specifiedType == ZoneType.CELL ?
          "info_player_terrorist" :
          "info_player_counterterrorist";

        var spawns = Utilities
         .FindAllEntitiesByDesignerName<SpawnPoint>(entName)
         .Where(s => s.AbsOrigin != null)
         .Select(s => s.AbsOrigin!);

        var generated = factory.CreateZone(spawns);
        generated.Draw(plugin, shapeFactory, specifiedType.Value.GetColor(),
          120);
        info.ReplyToCommand($"Drawing auto-generated {specifiedType} zone");
        return;
    }
  }

  private void attemptBeginCreation(CCSPlayerController executor,
    ZoneType type) {
    if (creators.ContainsKey(executor.SteamID)) {
      executor.PrintToChat("You are already creating a zone");
      return;
    }

    if (type.IsSinglePoint()) {
      if (executor.PlayerPawn.Value?.AbsOrigin != null) {
        var zone =
          factory.CreateZone([executor.PlayerPawn.Value.AbsOrigin!.Clone()]);
        zone.Draw(plugin, shapeFactory, type.GetColor(), 1f);
        // Server.NextFrameAsync(async () => {
        Task.Run(async () => {
          await zoneManager.PushZone(zone, type);
          await Server.NextFrameAsync(() => {
            executor.PrintToChat("Successfully created a single point zone");
          });
        });
      } else {
        executor.PrintToChat(
          "Unable to find your position. Please try again later.");
      }

      return;
    }

    var creator =
      new PlayerZoneCreator(plugin, executor, factory, type, shapeFactory);
    creator.BeginCreation();
    creators[executor.SteamID] = creator;
    executor.PrintToChat($"Began creation of a {type.ToString()} zone");
  }

  private (IZone, ZoneType)? getUniqueZone(CCSPlayerController player,
    ZoneType? type, bool print = true) {
    var zoneDictionary = zoneManager.GetAllZones().GetAwaiter().GetResult();

    if (type == null) {
      IZone?    result     = null;
      ZoneType? resultType = null;

      foreach (var zones in zoneDictionary) {
        var zone = zones.Value.FirstOrDefault(z
          => z.IsInsideZone(player.PlayerPawn.Value!.AbsOrigin!));
        if (zone == null) continue;
        if (result != null) {
          if (print) player.PrintToChat("Multiple zones found.");
          return null;
        }

        result     = zone;
        resultType = zones.Key;
      }

      if (result == null || resultType == null) {
        if (print) player.PrintToChat("No zones found");
        return null;
      }

      return (result, resultType.Value);
    }

    if (!zoneDictionary.TryGetValue(type.Value, out var value)) {
      if (print) player.PrintToChat("No zones found");
      return null;
    }

    var validZones = value.MinBy(z
      => z.GetMinDistanceSquared(player.PlayerPawn.Value!.AbsOrigin!));

    if (validZones == null) {
      if (print) player.PrintToChat("No zones found");
      return null;
    }

    return (validZones, type.Value);
  }

  private void sendUsage(CCSPlayerController player) {
    player.PrintToChat("Usage: css_zone [add/set/remove/tp] [type]");
    player.PrintToChat("css_zone [addinner/show/list] <type>");
    player.PrintToChat("css_zone [finish/cleanup/reload]");
    player.PrintToChat("css_zone generate [cell/armory]");
  }
}