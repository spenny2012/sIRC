namespace spennyIRC.Core.IRC;

public class IrcEvents : IIrcEvents
{
    public Dictionary<string, List<Func<IIrcReceivedContext, Task>>> EventSubscriptions { get; set; } = [];

    public async Task<bool> TryExecute(string ircEvent, IIrcReceivedContext ctx)
    {
        if (string.IsNullOrEmpty(ircEvent))
            throw new ArgumentNullException(nameof(ircEvent));

        if (EventSubscriptions.TryGetValue(ircEvent, out List<Func<IIrcReceivedContext, Task>>? eventList))
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                await eventList[i](ctx);
            }
            return true;
        }

        return false;
    }

    public void AddEvent(string ircEvent, Func<IIrcReceivedContext, Task> evt)
    {
        if (string.IsNullOrEmpty(ircEvent))
            throw new ArgumentNullException(nameof(ircEvent));

        ArgumentNullException.ThrowIfNull(evt);

        if (!EventSubscriptions.TryGetValue(ircEvent, out List<Func<IIrcReceivedContext, Task>>? eventList))
        {
            EventSubscriptions[ircEvent] = [evt];
            return;
        }

        eventList.Add(evt);
    }
}
