using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;
using spennyIRC.Scripting.Helpers;
using System;
using System.Text;
using System.Threading.Tasks;
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
        AddCommand("repeat", "repeat yourself", (p, session) =>
        {
            IrcCommandParametersInfo cmdParams = p.ExtractCommandParametersInfo();

            if (cmdParams.LineParts == null || !int.TryParse(cmdParams.LineParts[0], out int times)) return Task.CompletedTask;

            string? sentence = cmdParams.Parameters?.TrimStart(cmdParams.LineParts[0].ToCharArray()).Trim();

            return Repeat(session, sentence, times);
        });
    }

    public override void Execute()
    {
    }

    private async Task Repeat(IIrcSession session, string? sentence, int times)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return;

        for (int i = 0; i < times; i++)
        {
            await _commands.ExecuteCommand("say", sentence, session); 
        }
    }
}
