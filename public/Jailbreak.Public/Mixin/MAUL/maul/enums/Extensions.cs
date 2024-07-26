namespace maul.enums;

public static class Extensions
{
    public static string ToFriendlyString(this MaulPermission permission)
    {
        switch (permission)
        {
            case MaulPermission.None:
                return "None";
            case MaulPermission.E:
                return "=(e)=";
            case MaulPermission.EG:
                return "=(eG)=";
            case MaulPermission.EGO:
                return "=(eGO)=";
            case MaulPermission.Advisor:
                return "Advisor";
            case MaulPermission.Manager:
                return "Manager";
            case MaulPermission.SeniorManager:
                return "Senior Manager";
            case MaulPermission.CommunityManager:
                return "Community Manager";
            case MaulPermission.Director:
                return "Director";
            case MaulPermission.Executive:
                return "Executive";
            default:
                return "Unknown";
        }
    }

    public static string ToFriendlyString(this SecondaryGroup group)
    {
        switch (group)
        {
            case SecondaryGroup.Media:
                return "Media";
            case SecondaryGroup.MS:
                return "Member Service";
            case SecondaryGroup.AT:
                return "Admin Trainer";
            case SecondaryGroup.EC:
                return "Event Coord";
            case SecondaryGroup.RC:
                return "Recruitment Coord";
            case SecondaryGroup.Tech:
                return "Tech";
            case SecondaryGroup.Founder:
                return "Founder";
            case SecondaryGroup.TechServer:
                return "TechServer";
            case SecondaryGroup.TechSuperAdmin:
                return "TechSuperAdmin";
            default:
                return "Unknown";
        }
    }

    public static string GetDescription(this SecondaryGroup group)
    {
        switch (group)
        {
            case SecondaryGroup.MS:
                return
                    "Responsible for the acceptance and onboarding of new members and the overall experience of current members.";
            case SecondaryGroup.AT:
                return "Train new admins and oversee the creation of new rules and policies.";
            case SecondaryGroup.EC:
                return "Coordinate events and activities across the community.";
            case SecondaryGroup.RC:
                return "Manage recruitment and miscellaneous areas involving member retention.";
            case SecondaryGroup.Tech:
                return "Oversee and ensure the smooth operation of the community's technical infrastructure.";
            case SecondaryGroup.Founder:
                return "Find things";
            case SecondaryGroup.TechServer:
                return "Server-specific tech role";
            case SecondaryGroup.TechSuperAdmin:
                return "Really big brain tech role";
            default:
                return "Hmm...";
        }
    }
}