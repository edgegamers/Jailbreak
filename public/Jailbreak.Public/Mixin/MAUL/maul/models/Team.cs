namespace maul.models;

public class Team
{
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public int MaulApiId { get; set; }
    public ulong? RoleId { get; set; }
}