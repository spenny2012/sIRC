using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;
using spennyIRC.Scripting.Helpers; // Do not remove
using System;                      // Do not remove
using System.Text;                 // Do not remove
using System.Threading.Tasks;      // Do not remove
#pragma warning disable IDE0079    // Remove unnecessary suppression
#pragma warning disable CA1050     // Remove declare types in namespace
public class HelloWorldScript(IIrcCommands commands) : SircScript(commands)
{
    public override string Name => "Hello World Script"; // Required
    //public override string Version => "1.0";
    //public override string Author => "SK";
    //public override string Description => "A simple test script.";

    public override void Execute()
    {
    }

    public override void Initialize()
    {
        AddCommand("rcmd", "repeat command", (p, session) =>
        {
            IrcCommandParametersInfo cmdParams = p.ExtractCommandParameters();

            int? times = cmdParams.GetParam<int?>(0);
            if (!times.HasValue || times == 0) return Task.CompletedTask;

            return RepeatCmdAsync(session, cmdParams.Parameters!.GetTokenFrom(1), times.Value);
        });
    }

    private async Task RepeatCmdAsync(IIrcSession session, string command, int times = 20)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        IrcCommandInfo cmdParams = command.ExtractCommand();

        for (int i = 0; i < times; i++)
        {
            await _commands.ExecuteCommand(
                cmdParams.Command,
                cmdParams.Parameters,
                session);
        }
    }
}

//using spennyIRC.Scripting.Helpers;
//using System;
//using System.Text;
//using System.Threading.Tasks;