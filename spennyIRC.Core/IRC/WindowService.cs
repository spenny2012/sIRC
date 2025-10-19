namespace spennyIRC.Core.IRC;

// TODO: move this to viewmodels
public class WindowService : IWindowService
{
    public Action<string> DoClear { get; set; }
    public Action<string, string> DoEcho { get; set; }

    public void Clear(string window)
    {
        DoClear?.Invoke(window);
    }

    public void Echo(string window, string text)
    {
        DoEcho?.Invoke(window, text);
    }
}