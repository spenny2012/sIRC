namespace spennyIRC.Core.IRC;

public interface IIrcServer
{
    bool Connected { get; set; }
    string Host { get; set; }
    bool IsTls { get; set; }
    string Network { get; set; }
    string NetworkId { get; set; }
    string Port { get; set; }
    Dictionary<string, string> Settings { get; set; }

    void Clear();
}