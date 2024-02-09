namespace Jailbreak.Public.Mod.LastRequest;

public class GenericLastRequestOptions : LastRequestOptions
{
    public GenericLastRequestOptions(AbstractLastRequest baseRequest) : base(baseRequest)
    {
    }

    public override void GetOptions()
    {
    }

    public int GetMaxTime()
    {
        return 60;
    }
}