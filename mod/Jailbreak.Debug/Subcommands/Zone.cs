using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_zone [add/set/remove/tpto] [type]
// css_zone draw <type>
public class Zone(IServiceProvider services, BasePlugin plugin)
  : AbstractCommand(services) {
  private readonly IDictionary<ulong, ITypedZoneCreator> creators =
    new Dictionary<ulong, ITypedZoneCreator>();

  private readonly IZoneFactory factory =
    services.GetRequiredService<IZoneFactory>();

  private readonly IZoneManager zoneManager =
    services.GetRequiredService<IZoneManager>();

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
        Server.NextFrameAsync(async () => {
          await zoneManager.PushZone(zone, creator.Type);
          Server.NextFrame(() => {
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
          var displayZones =
            zoneManager.GetZones(type).GetAwaiter().GetResult();
          foreach (var z in displayZones) z.Draw(plugin, type.GetColor(), 120);

          zoneCount += displayZones.Count;
        }

        if (zoneCount == 0) {
          if (specifiedType.HasValue)
            info.ReplyToCommand($"No {specifiedType} zones found");
          else
            info.ReplyToCommand("No zones found");
          return;
        }

        if (specifiedType.HasValue)
          info.ReplyToCommand($"Showing {zoneCount} {specifiedType} zones");
        else
          info.ReplyToCommand($"Showing {zoneCount} zones");
        return;
      case "remove":
      case "delete":
        var toDelete = getUniqueZone(executor, specifiedType);
        if (toDelete == null) return;
        Server.NextFrameAsync(async () => {
          await zoneManager.DeleteZone(toDelete.Value.Item1.Id);
          Server.NextFrame(() => {
            executor.PrintToChat("Deleted zone #" + toDelete.Value.Item1.Id);
          });
        });

        return;
      case "addinner":
        var innerPair = getUniqueZone(executor, specifiedType);
        if (innerPair == null) return;
        var innerZone = innerPair.Value.Item1;
        innerZone.AddPoint(position);
        var map = Server.MapName;
        Server.NextFrameAsync(async () => {
          await zoneManager.DeleteZone(innerZone.Id);
          await zoneManager.PushZoneWithID(innerZone, innerPair.Value.Item2,
            map);
          Server.NextFrame(() => {
            info.ReplyToCommand("Added point to zone #" + innerZone.Id);
          });
        });
        return;
      case "reload":
      case "refresh":
        Server.NextFrameAsync(async () => {
          await zoneManager.LoadZones(Server.MapName);
          var count = (await zoneManager.GetAllZones()).SelectMany(e => e.Value)
           .Count();
          Server.NextFrame(() => {
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
            var zones = zoneManager.GetZones(type).GetAwaiter().GetResult();
            if (!allZones.ContainsKey(type)) continue;

            info.ReplyToCommand($"{type} zones: {zones.Count}");
          }

          return;
        }

        var toList = zoneManager.GetZones(specifiedType.Value)
         .GetAwaiter()
         .GetResult();
        foreach (var listZone in toList)
          info.ReplyToCommand(
            $"Points: {listZone.GetBorderPoints().Count()}/{listZone.GetAllPoints().Count()}Center: {listZone.CalculateCenterPoint()} Area: {listZone.GetArea()}");

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
        attemptBeginCreation(executor, info, specifiedType.Value);
        return;
      case "set":
        foreach (var zone in zoneManager.GetZones(specifiedType.Value)
         .GetAwaiter()
         .GetResult())
          zoneManager.DeleteZone(zone.Id);

        attemptBeginCreation(executor, info, specifiedType.Value);
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
          => a.GetMinDistance(position) <= b.GetMinDistance(position) ? -1 : 1);
        executor.PlayerPawn.Value.Teleport(tpDestinations.First()
         .GetCenterPoint());

        info.ReplyToCommand("Teleported to zone #" + tpDestinations.First().Id);
        return;
    }
  }

  private void attemptBeginCreation(CCSPlayerController executor,
    WrappedInfo info, ZoneType type) {
    if (creators.ContainsKey(executor.SteamID)) {
      executor.PrintToChat("You are already creating a zone");
      return;
    }

    if (type.IsSinglePoint()) {
      if (executor.PlayerPawn.Value?.AbsOrigin != null) {
        var zone = factory.CreateZone([executor.PlayerPawn.Value.AbsOrigin!]);
        zone.Draw(plugin, type.GetColor(), 1f);
        Server.NextFrameAsync(async () => {
          await zoneManager.PushZone(zone, type);
          Server.NextFrame(() => {
            executor.PrintToChat("Successfully created a single point zone");
          });
        });
      } else {
        executor.PrintToChat(
          "Unable to find your position. Please try again later.");
      }

      return;
    }

    var creator = new PlayerZoneCreator(plugin, executor, factory, type);
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
          player.PrintToChat("Multiple zones found.");
          return null;
        }

        result     = zone;
        resultType = zones.Key;
      }

      if (result == null || resultType == null) {
        player.PrintToChat("No zones found");
        return null;
      }

      return (result, resultType.Value);
    }

    if (!zoneDictionary.TryGetValue(type.Value, out var value)) {
      player.PrintToChat("No zones found");
      return null;
    }

    var validZones = value.Where(zone
        => zone.IsInsideZone(player.PlayerPawn.Value!.AbsOrigin!))
     .ToList();

    switch (validZones.Count) {
      case 0:
        player.PrintToChat("No zones found");
        return null;
      case > 1:
        player.PrintToChat("Multiple zones found.");
        return null;
      default:
        return (validZones.First()!, type.Value);
    }
  }

  private void sendUsage(CCSPlayerController player) {
    player.PrintToChat("Usage: css_zone [add/set/remove/tp] [type]");
    player.PrintToChat(ChatColors.Default
      + "       css_zone [addinner/show/list] <type>");
    player.PrintToChat(ChatColors.Default + "       css_zone [finish/reload]");
  }
}