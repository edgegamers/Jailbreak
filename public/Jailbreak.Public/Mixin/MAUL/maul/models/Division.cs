namespace maul.models;

public class Division
{
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public string? MaulApiName { get; set; }
    public ulong? RoleId { get; set; }
    public ulong? LeRoleId { get; set; }
}