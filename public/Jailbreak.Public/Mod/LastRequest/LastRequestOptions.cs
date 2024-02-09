namespace Jailbreak.Public.Mod.LastRequest;

public abstract class LastRequestOptions
{
    protected AbstractLastRequest baseRequest;
    
    public LastRequestOptions(AbstractLastRequest baseRequest)
    {
        this.baseRequest = baseRequest;
    }
    
    public abstract void GetOptions();
}