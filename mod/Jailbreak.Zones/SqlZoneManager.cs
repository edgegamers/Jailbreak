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
    Server.NextFrameAsync(createTable);

    basePlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    basePlugin.RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
  }

  public void Dispose() {
    plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
  }

  public async void DeleteZone(int zoneId, string map) {
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
        DELETE FROM {CvSqlTable.Value}
        WHERE zoneid = @zoneid
        AND map = @map
      """;

    cmd.Parameters.AddWithValue("@zoneid", zoneId);
    cmd.Parameters.AddWithValue("@map", map);
    await cmd.ExecuteNonQueryAsync();
  }

  public async void PushZoneWithID(IZone zone, ZoneType type, string map) {
    if (!zones.TryGetValue(type, out var list)) {
      list        = new List<IZone>();
      zones[type] = list;
    }

    list.Add(zone);

    var conn = createConnection();
    if (conn == null) return;
    await conn.OpenAsync();

    var insertPointCommand = $"""
        INSERT INTO {CvSqlTable.Value} (map, type, zoneid, pointid, X, Y, Z)
        VALUES (@map, @type, @zoneid, @pointid, @X, @Y, @Z)
      """;
    var cmd = conn.CreateCommand();
    cmd.CommandText = insertPointCommand;

    cmd.Parameters.AddWithValue("@map", map);
    cmd.Parameters.AddWithValue("@type", type.ToString());

    cmd.Parameters.AddWithValue("@zoneid", zone.Id);
    var pointId = 0;

    foreach (var point in zone.GetBorderPoints()) {
      cmd.Parameters.AddWithValue("@X", point.X);
      cmd.Parameters.AddWithValue("@Y", point.Y);
      cmd.Parameters.AddWithValue("@Z", point.Z);
      cmd.Parameters.AddWithValue("@pointid", pointId++);
      await cmd.ExecuteNonQueryAsync();
    }
  }

  public Task<IList<IZone>> GetZones(string map, ZoneType type) {
    return Task.FromResult(!zones.TryGetValue(type, out var result) ?
      new List<IZone>() :
      result);
  }

  public async void PushZone(IZone zone, ZoneType type, string map) {
    if (!zones.TryGetValue(type, out var list)) {
      list        = new List<IZone>();
      zones[type] = list;
    }

    list.Add(zone);

    var nextId = zones.SelectMany(s => s.Value).Max(z => z.Id) + 1;
    zone.Id = nextId;
    PushZoneWithID(zone, type, map);
  }

  public async void UpdateZone(IZone zone, ZoneType type, int id) {
    DeleteZone(id, Server.MapName);
    zone.Id = id;
    PushZoneWithID(zone, type, Server.MapName);
  }

  public Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones() {
    return Task.FromResult(zones);
  }

  public async void LoadZones(string map) {
    var tasks = Enum.GetValues<ZoneType>()
     .Select(type => LoadZones(map, type))
     .ToList();

    await Task.WhenAll(tasks);
  }

  private void OnMapEnd() { zones.Clear(); }

  private async void createTable() {
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
  }

  private void OnMapStart(string mapname) {
    Server.NextFrameAsync(() => LoadZones(mapname));
  }

  private MySqlCommand queryAllZones(string map) {
    var cmdText = $"""
          SELECT * FROM {CvSqlTable.Value}
          WHERE map = '{map}'
          ORDER BY zoneid, pointid DESC
      """;

    return new MySqlCommand(cmdText);
  }

  public async Task LoadZones(string map, ZoneType type) {
    var conn = createConnection();
    if (conn == null) return;

    var cmd = queryAllZones(map);

    await conn.OpenAsync();
    cmd.Connection = conn;

    var reader = await cmd.ExecuteReaderAsync();

    var currentZone = -1;
    var pointId     = -1;
    var zoneCreator = new BasicZoneCreator();
    zoneCreator.BeginCreation();
    while (await reader.ReadAsync()) {
      var point = new Vector(reader.GetFloat("X"), reader.GetFloat("Y"),
        reader.GetFloat("Z"));
      zoneCreator.AddPoint(point);
      var zoneId = reader.GetInt32("zoneid");
      pointId = reader.GetInt32("pointid");

      // We just started reading zones
      if (currentZone == -1) currentZone = zoneId;

      if (pointId == 0 || zoneId != currentZone) {
        if (pointId == 0 != (zoneId != currentZone))
          printNotClosedWarning(map, zoneId, pointId, currentZone);
        // Assume the zone is closed and allow the new zone to be created
        var zone                                  = zoneCreator.Build(factory);
        if (!zones.ContainsKey(type)) zones[type] = new List<IZone>();
        zone.Id = zoneId;
        zones[type].Add(zone);
        currentZone = zoneId;
        zoneCreator.BeginCreation();
      }
    }

    if (pointId > 0) printNotClosedWarning(map, -1, pointId, currentZone);
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