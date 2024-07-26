namespace maul.api.models;

public interface IMaulSettings
{
    string Url { get; set; }
    string Token { get; set; }
    string IpAddress { get; set; }
    int Port { get; set; }
}