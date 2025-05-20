namespace spennyIRC.Core.IRC;

public class IrcServerInfo : IIrcServer
{
    public string Host { get; set; }
    public string Port { get; set; }
    public string Network { get; set; }
    public string NetworkId { get; set; }
    public bool IsTls { get; set; }
    public bool Connected { get; set; }
    public Dictionary<string, string> Settings { get; set; } = [];
}
