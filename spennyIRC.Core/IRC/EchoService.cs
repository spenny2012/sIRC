namespace spennyIRC.Core.IRC;

// TODO: move this to viewmodels
public class EchoService : IEchoService
{
    public Action<string, string> DoEcho { get; set; }

    public void Echo(string window, string text)
    {
        DoEcho?.Invoke(window, text);
    }
}
