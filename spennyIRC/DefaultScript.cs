using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;
using spennyIRC.Scripting.Helpers; // Do not remove
using System;                      // Do not remove
using System.IO;
using System.Text;                 // Do not remove
using System.Threading;
using System.Threading.Tasks;      // Do not remove
#pragma warning disable IDE0079    // Remove unnecessary suppression
#pragma warning disable CA1050     // Remove declare types in namespace
public class HelloWorldScript(IIrcCommands commands) : SircScript(commands)
{
    public override string Name => "Hello World Script"; // Required

    private static CancellationTokenSource? _playCancellationTokenSource;
    private bool _isPlaying;

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
            if (!times.HasValue || times <= 0) return Task.CompletedTask;

            return RepeatCmdAsync(session, cmdParams.Parameters!.GetTokenFrom(1), times.Value);
        });

        AddCommand("play", "play a .txt file", async (p, session) =>
        {
            IrcCommandParametersInfo cmdParams = p.ExtractCommandParameters();

            string? path = cmdParams.GetParam<string?>(0);
            int? delay = cmdParams.GetParam<int?>(1);

            if (path?.Equals("stop", StringComparison.OrdinalIgnoreCase) == true && _playCancellationTokenSource != null)
            {
                await _playCancellationTokenSource.CancelAsync();
                return;
            }

            else if (!delay.HasValue || delay <= 0 || string.IsNullOrWhiteSpace(path))
                return;

            else if (_isPlaying)
            {
                session.WindowService.Echo(session.ActiveWindow, $"* Another file is already playing. Type `/play stop` to halt it.");
                return;
            }

            _isPlaying = true;
            _playCancellationTokenSource = new();
            await PlayAsync(session, path, _playCancellationTokenSource.Token, delay.Value);
            _isPlaying = false;
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

    private async Task PlayAsync(IIrcSession session, string filePath, CancellationToken cancellationToken, int delay = 5)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));

        if (!Path.Exists(filePath))
        {
            session.WindowService.Echo(session.ActiveWindow, $"* File not found '{filePath}'");
            return;
        }

        session.WindowService.Echo(session.ActiveWindow, $"* Playing '{filePath}' with a {delay} second delay.");

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            await _commands.ExecuteCommand("say", line, session);

            if (delay == 0) continue;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken); // TODO: change to ms
            }
            catch
            {
                break;
            }
        }

        session.WindowService.Echo(session.ActiveWindow, $"* Finished playing '{filePath}'");
    }
}

//using spennyIRC.Scripting.Helpers;
//using System;
//using System.Text;
//using System.Threading.Tasks;
