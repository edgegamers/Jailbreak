using System.Text;
using maul.enums;

namespace maul.models;

public class UserInfo
{
    public int UserId { get; set; }
    public string? GameId { get; set; }
    public string? Name { get; set; }
    public string? Error { get; set; }
    public int State { get; set; }
    public string? DivisionName { get; set; }
    public string? Division { get; set; }
    public string? PrimaryGroup { get; set; }
    public MaulPermission PrimaryRank { get; set; }
    public IEnumerable<UserInfoGroup>? Groups { get; set; }
    public UserInfoDs? DsInfo { get; set; }
    public bool Verification { get; set; }
    public bool VerificationExpired { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"UserId: {UserId}");
        sb.AppendLine($"GameId: {GameId}");
        sb.AppendLine($"Name: {Name}");
        sb.AppendLine($"Error: {Error}");
        sb.AppendLine($"State: {State}");
        sb.AppendLine($"DivisionName: {DivisionName}");
        sb.AppendLine($"Division: {Division}");
        sb.AppendLine($"PrimaryGroup: {PrimaryGroup}");
        sb.AppendLine($"PrimaryRank: {PrimaryRank}");
        sb.AppendLine($"Verification: {Verification}");
        sb.AppendLine($"VerificationExpired: {VerificationExpired}");

        if (Groups != null)
        {
            sb.AppendLine("Groups:");
            foreach (var group in Groups) sb.AppendLine($"    {group}");
        }

        if (DsInfo != null) sb.AppendLine($"DsInfo: {DsInfo}");

        return sb.ToString();
    }
}