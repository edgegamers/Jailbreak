namespace maul.models;

public class DS
{
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public int MaulApiTier { get; set; }
    public ulong? RoleId { get; set; }
    public ulong? HoistedRoleId { get; set; }
}