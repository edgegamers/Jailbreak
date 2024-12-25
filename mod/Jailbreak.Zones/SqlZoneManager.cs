using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Public.Mod.Zones;
using MySqlConnector;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Zones;

public class SqlZoneManager(IZoneFactory factory) : IZoneManager {
  public static readonly FakeConVar<string> CV_SQL_TABLE =
    new("css_jb_zonetable", "The table name for the zones", "cs2_jb_zones");

  public static readonly FakeConVar<string> CV_SQL_CONNECTION_STRING =
    new("css_jb_sqlconnection", "The connection string for the database", "",
      ConVarFlags.FCVAR_PROTECTED);

  private readonly Dictionary<ZoneType, IList<IZone>> zones = new();

  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin, bool hotReload) {
    plugin = basePlugin;

    if (!hotReload)
      CV_SQL_CONNECTION_STRING.ValueChanged += async (_, _) => {
        await LoadZones(Server.MapName);
      };

    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    basePlugin.RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
  }

  void IDisposable.Dispose() {
    plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
    plugin.RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
  }

  public async Task DeleteZone(int zoneId, string map) {
    foreach (var list in zones.Values) {
      var zone = list.FirstOrDefault(z => z.Id == zoneId);
      if (zone == null) continue;
      list.Remove(zone);
      break;
    }

    foreach (var list in zones.Values) {
      var zone = list.FirstOrDefault(z => z.Id == zoneId);
      if (zone == null) continue;
      list.Remove(zone);
      break;
    }

    await using var conn = createConnection();
    if (conn == null) return;
    try {
      await conn.OpenAsync();
      await using var cmd = conn.CreateCommand();
      cmd.CommandText = $"""
          DELETE FROM {CV_SQL_TABLE.Value.Trim('"')}
          WHERE zoneid = @zoneid
          AND map = @map
        """;

      cmd.Parameters.AddWithValue("@zoneid", zoneId);
      cmd.Parameters.AddWithValue("@map", map);
      await cmd.ExecuteNonQueryAsync();
    } catch (MySqlException e) { Console.WriteLine(e); } finally {
      await conn.CloseAsync();
    }
  }

  public async Task PushZoneWithID(IZone zone, ZoneType type, string map) {
    if (!zones.TryGetValue(type, out var list)) {
      list        = new List<IZone>();
      zones[type] = list;
    }

    // remove other zones with the same id
    foreach (var zoneList in zones.Values) {
      var z = zoneList.FirstOrDefault(z => z.Id == zone.Id);
      if (z == null) continue;
      zoneList.Remove(z);
      break;
    }

    list.Add(zone);

    await using var conn = createConnection();
    if (conn == null) return;
    try {
      await conn.OpenAsync();

      var insertPointCommand = $"""
          INSERT INTO {CV_SQL_TABLE.Value.Trim('"')} (map, type, zoneid, pointid, X, Y, Z)
          VALUES (@map, @type, @zoneid, @pointid, @X, @Y, @Z)
        """;
      var pointId = 0;

      foreach (var point in zone.GetAllPoints()) {
        await using var cmd = new MySqlCommand(insertPointCommand, conn);
        cmd.Parameters.AddWithValue("@map", map);
        cmd.Parameters.AddWithValue("@type", type.ToString());

        cmd.Parameters.AddWithValue("@zoneid", zone.Id);
        cmd.Parameters.AddWithValue("@X", point.X);
        cmd.Parameters.AddWithValue("@Y", point.Y);
        cmd.Parameters.AddWithValue("@Z", point.Z);
        cmd.Parameters.AddWithValue("@pointid", pointId++);
        await cmd.ExecuteNonQueryAsync();
      }
    } catch (MySqlException e) { Console.WriteLine(e); } finally {
      await conn.CloseAsync();
    }
  }

  public Task<IList<IZone>> GetZones(string map, ZoneType type) {
    return Task.FromResult(!zones.TryGetValue(type, out var result) ?
      new List<IZone>() :
      result);
  }

  public async Task PushZone(IZone zone, ZoneType type, string map) {
    if (!zones.TryGetValue(type, out var list)) {
      list        = new List<IZone>();
      zones[type] = list;
    }

    var allZones = zones.Values.SelectMany(s => s).ToList();

    var nextId = allZones.Count == 0 ? 0 : allZones.Max(z => z.Id) + 1;
    zone.Id = nextId;
    await PushZoneWithID(zone, type, map);
  }

  public async Task UpdateZone(IZone zone, ZoneType type, int id, string map) {
    await DeleteZone(id, map);
    zone.Id = id;
    await PushZoneWithID(zone, type, map);
  }

  public Task<Dictionary<ZoneType, IList<IZone>>> GetAllZones() {
    return Task.FromResult(zones);
  }

  public async Task LoadZones(string map) {
    zones.Clear();
    var tasks = Enum.GetValues<ZoneType>()
     .Select(type => LoadZones(map, type))
     .ToList();

    await Task.WhenAll(tasks);
  }

  private void OnMapEnd() { zones.Clear(); }

  private async Task createTable() {
    await using var conn = createConnection();
    if (conn == null) return;
    try {
      await conn.OpenAsync();

      var cmdText = $"""
        CREATE TABLE IF NOT EXISTS {CV_SQL_TABLE.Value.Trim('"')}(
          zoneid INT NOT NULL,
          pointid INT NOT NULL,
          map VARCHAR(64) NOT NULL,
          type VARCHAR(32) NOT NULL,
          X FLOAT NOT NULL,
          Y FLOAT NOT NULL,
          Z FLOAT NOT NULL,
          PRIMARY KEY(map, zoneid, pointid))
        """;

      await using var cmd = new MySqlCommand(cmdText, conn);
      await cmd.ExecuteNonQueryAsync();
    } catch (MySqlException e) { Console.WriteLine(e); } finally {
      await conn.CloseAsync();
    }
  }

  private void OnMapStart(string mapname) {
    Task.Run(async () => {
      await createTable();
      await LoadZones(mapname);
    });
  }

  private MySqlCommand queryAllZones(MySqlConnection conn, string map,
    ZoneType type) {
    var cmdText = $"""
          SELECT * FROM {CV_SQL_TABLE.Value.Trim('"')}
          WHERE map = '{map}' AND type = '{type}'
          ORDER BY zoneid, pointid DESC
      """;

    return new MySqlCommand(cmdText, conn);
  }

  public async Task LoadZones(string map, ZoneType type) {
    // var conn = createConnection();
    await using var conn = createConnection();
    if (conn == null) return;

    try {
      await conn.OpenAsync();
      await using var cmd         = queryAllZones(conn, map, type);
      await using var reader      = await cmd.ExecuteReaderAsync();
      var             currentZone = -1;
      var             pointId     = -1;
      var             zoneCreator = new BasicZoneCreator();
      zoneCreator.BeginCreation();
      while (await reader.ReadAsync()) {
        var x      = reader.GetFloat("X");
        var y      = reader.GetFloat("Y");
        var z      = reader.GetFloat("Z");
        var zoneId = reader.GetInt32("zoneid");
        var pid    = reader.GetInt32("pointid");
        Server.NextFrame(() => {
          var point = new Vector(x, y, z);
          zoneCreator.AddPoint(point);
          // We just started reading zones
          if (currentZone == -1) currentZone = zoneId;

          if (pid != 0 && zoneId == currentZone) return;
          if (zoneId != currentZone)
            printNotClosedWarning(map, zoneId, pid, currentZone);
          // Assume the zone is closed and allow the new zone to be created
          var zone = zoneCreator.Build(factory);
          if (!zones.ContainsKey(type)) zones[type] = new List<IZone>();
          zone.Id = zoneId;
          zones[type].Add(zone);
          pointId     = -1;
          currentZone = -1;
          zoneCreator.BeginCreation();
        });
      }

      if (pointId > 0) printNotClosedWarning(map, -1, pointId, currentZone);
    } catch (MySqlException e) { Console.WriteLine(e); } finally {
      await conn.CloseAsync();
    }

    await Task.Delay((int)Math.Ceiling(Server.TickInterval / 1000) * 5);
  }

  private void printNotClosedWarning(string map, int zoneId, int pointId,
    int currentZone) {
    if (zoneId == -1) {
      Server.PrintToConsole(
        "[WARNING] Expected a closing point but read end of table");
      Server.PrintToConsole(
        $"{map} Read: {zoneId} {pointId} Current Zone: {currentZone}");
      return;
    }

    Server.PrintToConsole("[WARNING] Encountered a new zone introduction but "
      + "the previous zone was not closed properly");
    Server.PrintToConsole(
      $"{map} Read: {zoneId} {pointId} Current Zone: {currentZone}");
  }

  private MySqlConnection? createConnection() {
    var str = CV_SQL_CONNECTION_STRING.Value.Trim('"');
    return string.IsNullOrWhiteSpace(str) ? null : new MySqlConnection(str);
  }
}