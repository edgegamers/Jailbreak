namespace api.plugin.services;

public interface IRenamer
{
    Dictionary<ulong, string> cachedNames { get; }
}