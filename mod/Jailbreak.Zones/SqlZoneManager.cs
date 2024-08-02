using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Public.Mod.Zones;
using MySqlConnector;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Zones;

public class SqlZoneManager(IZoneFactory factory) : IZoneManager {
  public static FakeConVar<string> CvSqlTable = new("css_jb_zonetable",
    "The table name for the zones", "cs2_jb_zones");

  public static FakeConVar<string> CvSqlConnectionString =
    new("css_jb_sqlconnection", "The connection string for the database", "",
      ConVarFlags.FCVAR_PROTECTED);

  private readonly IDictionary<ZoneType, IList<IZone>> zones =
    new Dictionary<ZoneType, IList<IZone>>();

  private BasePlugin plugin = null!;

  public void Start(BasePlugin basePlugin) {
    plugin = basePlugin;
    Server.NextFrameAsync(async () => { await createTable(); });

    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    basePlugin.RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
  }

  public void Dispose() {
    plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  public async Task DeleteZone(int zoneId, string map) {
    foreach (var list in zones.Values) {
      var zone = list.FirstOrDefault(z => z.Id == zoneId);
      if (zone != null) {
        list.Remove(zone);
        break;
      }
    }

    var conn = createConnection();
    if (conn == null) return;
    await conn.OpenAsync();
    var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
        DELETE FROM {CvSqlTable.Value.Trim('"')}
        WHERE zoneid = @zoneid
        AND map = @map
      """;

    cmd.Parameters.AddWithValue("@zoneid", zoneId);
    cmd.Parameters.AddWithValue("@map", map);
    await cmd.ExecuteNonQueryAsync();

    await conn.CloseAsync();
  }

  public async Task PushZoneWithID(IZone zone, ZoneType type, string map) {
    Server.NextFrame(() => { Server.PrintToConsole("Pushing zone to SQL"); });
    if (!zones.TryGetValue(type, out var list)) {
      list        = new List<IZone>();
      zones[type] = list;
    }

    list.Add(zone);

    var conn = createConnection();
    if (conn == null) return;
    Server.NextFrame(() => {
      Server.PrintToConsole("Successfully connect to sql");
    });
    await conn.OpenAsync();

    var insertPointCommand = $"""
        INSERT INTO {CvSqlTable.Value.Trim('"')} (map, type, zoneid, pointid, X, Y, Z)
        VALUES (@map, @type, @zoneid, @pointid, @X, @Y, @Z)
      """;
    Server.NextFrame(() => { Server.PrintToConsole("Creating command"); });
    var pointId = 0;

    foreach (var point in zone.GetAllPoints()) {
      var cmd = conn.CreateCommand();
      cmd.CommandText = insertPointCommand;

      cmd.Parameters.AddWithValue("@map", map);
      cmd.Parameters.AddWithValue("@type", type.ToString());

      cmd.Parameters.AddWithValue("@zoneid", zone.Id);
      cmd.Parameters.AddWithValue("@X", point.X);
      cmd.Parameters.AddWithValue("@Y", point.Y);
      cmd.Parameters.AddWithValue("@Z", point.Z);
      cmd.Parameters.AddWithValue("@pointid", pointId++);
      await cmd.ExecuteNonQueryAsync();
    }

    await conn.CloseAsync();
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

    list.Add(zone);

    var nextId = zones.SelectMany(s => s.Value).Max(z => z.Id) + 1;
    zone.Id = nextId;
    await PushZoneWithID(zone, type, map);
  }

  public async Task UpdateZone(IZone zone, ZoneType type, int id) {
    await DeleteZone(id, Server.MapName);
    zone.Id = id;
    await PushZoneWithID(zone, type, Server.MapName);
  }

  public Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones() {
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
    Server.PrintToConsole("Creating table for zones with auth: "
      + CvSqlConnectionString.Value);
    var conn = createConnection();
    if (conn == null) return;
    await conn.OpenAsync();

    var cmdText = $"""
      CREATE TABLE IF NOT EXISTS {CvSqlTable.Value.Trim('"')}(
        zoneid INT NOT NULL,
        pointid INT NOT NULL,
        map VARCHAR(64) NOT NULL,
        type VARCHAR(32) NOT NULL,
        X FLOAT NOT NULL,
        Y FLOAT NOT NULL,
        Z FLOAT NOT NULL,
        PRIMARY KEY(map, zoneid, pointid))
      """;

    var cmd = new MySqlCommand(cmdText, conn);
    await cmd.ExecuteNonQueryAsync();
    await conn.CloseAsync();
  }

  private void OnMapStart(string mapname) {
    Server.NextFrameAsync(async () => await LoadZones(mapname));
  }

  private MySqlCommand queryAllZones(string map, ZoneType type) {
    var cmdText = $"""
          SELECT * FROM {CvSqlTable.Value.Trim('"')}
          WHERE map = '{map}' AND type = '{type}'
          ORDER BY zoneid, pointid DESC
      """;

    return new MySqlCommand(cmdText);
  }

  public async Task LoadZones(string map, ZoneType type) {
    var conn = createConnection();
    if (conn == null) return;

    var cmd = queryAllZones(map, type);

    await conn.OpenAsync();
    cmd.Connection = conn;

    var reader = await cmd.ExecuteReaderAsync();

    var currentZone = -1;
    var pointId     = -1;
    var zoneCreator = new BasicZoneCreator();
    zoneCreator.BeginCreation();
    Server.NextFrame(() => {
      while (reader.Read()) {
        var point = new Vector(reader.GetFloat("X"), reader.GetFloat("Y"),
          reader.GetFloat("Z"));
        zoneCreator.AddPoint(point);
        var zoneId = reader.GetInt32("zoneid");
        pointId = reader.GetInt32("pointid");

        // We just started reading zones
        if (currentZone == -1) currentZone = zoneId;

        if (pointId == 0 || zoneId != currentZone) {
          if (zoneId != currentZone)
            printNotClosedWarning(map, zoneId, pointId, currentZone);
          // Assume the zone is closed and allow the new zone to be created
          var zone = zoneCreator.Build(factory);
          if (!zones.ContainsKey(type)) zones[type] = new List<IZone>();
          zone.Id = zoneId;
          zones[type].Add(zone);
          pointId     = -1;
          currentZone = -1;
          zoneCreator.BeginCreation();
        }
      }

      reader.Close();

      if (pointId > 0) printNotClosedWarning(map, -1, pointId, currentZone);
    });
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
    var str = CvSqlConnectionString.Value.Trim('"');
    if (string.IsNullOrWhiteSpace(str)) return null;
    return new MySqlConnection(str);
  }
}