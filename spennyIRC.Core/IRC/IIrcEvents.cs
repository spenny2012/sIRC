namespace spennyIRC.Core.IRC;

public interface IIrcEvents
{
    Dictionary<string, List<Func<IIrcReceivedContext, Task>>> EventSubscriptions { get; set; }

    void AddEvent(string ircEvent, Func<IIrcReceivedContext, Task> evt);

    Task<bool> TryExecute(string ircEvent, IIrcReceivedContext ctx);
}