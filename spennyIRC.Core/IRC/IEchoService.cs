namespace spennyIRC.Core.IRC;

// TODO: move this to viewmodels
public interface IEchoService
{
    public Action<string, string> DoEcho { get; set; }

    void Echo(string window, string text);
}