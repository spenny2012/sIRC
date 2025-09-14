namespace spennyIRC.Core.IRC;

public interface IIrcServer
{
    string Host { get; set; }
    string Port { get; set; }
    string Network { get; set; }
    string NetworkId { get; set; }
    bool IsTls { get; set; }
    bool Connected { get; set; }
    Dictionary<string, string> Settings { get; set; }
    void Clear();
}