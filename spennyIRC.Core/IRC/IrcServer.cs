namespace spennyIRC.Core.IRC;

public class IrcServer : IIrcServer
{
    public bool Connected { get; set; }
    public string Host { get; set; } = string.Empty;
    public bool IsTls { get; set; }
    public string Network { get; set; } = string.Empty;
    public string NetworkId { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public Dictionary<string, string> Settings { get; set; } = [];

    public void Reset()
    {
        Connected = false;
        IsTls = false;
        Host = string.Empty;
        Port = string.Empty;
        Network = string.Empty;
        NetworkId = string.Empty;
        Settings.Clear();
    }
}