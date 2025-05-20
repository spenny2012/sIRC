namespace spennyIRC.ViewModels.Messages;

public abstract class MessageBase(ISpennyIrcInstance session)
{
    public ISpennyIrcInstance Session => session;
}