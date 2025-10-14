namespace spennyIRC.Core.IRC;

// TODO: move this to viewmodels
public interface IWindowService
{
    Action<string, string> DoEcho { get; set; }

    void Echo(string window, string text);
}