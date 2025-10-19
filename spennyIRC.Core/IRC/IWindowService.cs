namespace spennyIRC.Core.IRC;

// TODO: move this to viewmodels
public interface IWindowService
{
    Action<string, string> DoEcho { get; set; }
    Action<string> DoClear { get; set; }

    void Echo(string window, string text);

    void Clear(string window);
}