using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;
using System;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1050  // Remove declare types in namespace
public class HelloWorldScript(IIrcCommands commands) : SircScript(commands)
{
    public override string Name => "Hello World Script"; // Required
    //public override string Version => "1.0";
    //public override string Author => "SK";
    //public override string Description => "A simple test script.";

    public override void Initialize()
    {
        AddCommand("hello", "say hello", (p, session) =>
        {
            //session.Client.SendMessageAsync("PRIVMSG"
            _commands.ExecuteCommand("say", "HELLO!!!", session);
            return Task.CompletedTask;
        });
    }

    public override void Execute()
    {
    }
}