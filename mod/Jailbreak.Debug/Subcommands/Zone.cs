using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_zone [add/set/remove/tpto] [type]
// css_zone draw <type>
public class Zone(IServiceProvider services, BasePlugin plugin)
  : AbstractCommand(services) {
  private IDictionary<ulong, ITypedZoneCreator> creators =
    new Dictionary<ulong, ITypedZoneCreator>();

  private IZoneManager
    zoneManager = services.GetRequiredService<IZoneManager>();

  private IZoneFactory factory = services.GetRequiredService<IZoneFactory>();

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
      info.ReplyToCommand("Usage: css_zone [add/set/tpto] [type]");
      info.ReplyToCommand("       css_zone remove <type>");
      info.ReplyToCommand("       css_zone finish");
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
            if (!executor.IsValid) return;

            executor.PrintToChat($"Successfully created {creator.Type} zone");
            creator.Dispose();
            creators.Remove(executor.SteamID);
          });
        });
        return;
      case "show":
        int zoneCount = 0;
        foreach (var type in Enum.GetValues<ZoneType>()) {
          if (specifiedType != null && type != specifiedType) continue;
          var displayZones =
            zoneManager.GetZones(type).GetAwaiter().GetResult();
          foreach (var z in displayZones) {
            z.Draw(plugin, type.GetColor(), 120);
          }

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
        var zoneDictionary = zoneManager.GetAllZones(Server.MapName)
         .GetAwaiter()
         .GetResult();

        var validZones = zoneDictionary
         .Where(s => specifiedType == null || s.Key == specifiedType)
         .SelectMany(s => s.Value)
         .Where(z => z.IsInsideZone(position))
         .ToList();

        if (validZones.Count == 0) {
          info.ReplyToCommand("No zones found");
          return;
        }

        if (validZones.Count > 1) {
          info.ReplyToCommand("Multiple zones found.");
          return;
        }

        var toDelete = validZones.First();
        zoneManager.DeleteZone(toDelete.Id);
        info.ReplyToCommand("Deleted zone #" + toDelete.Id);
        return;
    }

    if (info.ArgCount != 3) {
      info.ReplyToCommand("Usage: css_zone [add/set/remove/tpto] [type]");
      info.ReplyToCommand("       css_zone show <type>");
      info.ReplyToCommand("       css_zone finish");
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
         .GetResult()) { zoneManager.DeleteZone(zone.Id); }

        attemptBeginCreation(executor, info, specifiedType.Value);
        return;
      case "tpto":
        var tpDestinations = zoneManager.GetAllZones(Server.MapName)
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
        executor.Teleport(tpDestinations.First().GetCenterPoint());
        return;
    }
  }

  private void attemptBeginCreation(CCSPlayerController executor,
    WrappedInfo info, ZoneType type) {
    if (creators.ContainsKey(executor.SteamID)) {
      info.ReplyToCommand("You are already creating a zone");
      return;
    }

    if (type.IsSinglePoint()) {
      if (executor.PlayerPawn.Value?.AbsOrigin != null) {
        var zone = factory.CreateZone([executor.PlayerPawn.Value.AbsOrigin!]);
        zone.Draw(plugin, type.GetColor(), 1f);
        executor.ExecuteClientCommandFromServer("css_debug zone finish");
      } else {
        info.ReplyToCommand(
          "Unable to find your position. Please try again later.");
      }

      return;
    }

    var creator = new PlayerZoneCreator(plugin, executor, factory, type);
    creator.BeginCreation();
    creators[executor.SteamID] = creator;
    info.ReplyToCommand($"Began creation of a {type.ToString()} zone");
  }
}