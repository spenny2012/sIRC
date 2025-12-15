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
public class DefaultScript(IIrcCommands commands) : SircScript(commands)
{
    public override string Name => "Default sIRC Script"; // Required

    private static CancellationTokenSource? _playCancellationTokenSource;
    private bool _isPlaying;

    //public override string Version => "1.0";
    //public override string Author => "SK";
    //public override string Description => "A simple test script.";

    public override void Execute()
    {
    }

    /// <summary>
    /// Registers script commands with the sIRC command system.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if required command infrastructure provided by the base script is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// May be thrown if command registration fails due to duplicate command names
    /// or an invalid script state.
    /// </exception>
    /// <exception cref="Exception">
    /// Propagates any unexpected exceptions thrown during command registration
    /// or delegate execution.
    /// </exception>
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
            int delay = cmdParams.GetParam<int?>(1) ?? 1;

            if (path?.Equals("stop", StringComparison.OrdinalIgnoreCase) == true && _playCancellationTokenSource != null)
            {
                await _playCancellationTokenSource.CancelAsync();
                return;
            }

            if (delay < 0 || string.IsNullOrWhiteSpace(path))
                return;

            if (_isPlaying)
            {
                session.WindowService.Echo(session.ActiveWindow, $"* Another file is already playing. Type `/play stop` to halt it.");
                return;
            }

            _isPlaying = true;
            _playCancellationTokenSource = new();
            await PlayAsync(session, path, _playCancellationTokenSource.Token, delay);
            _isPlaying = false;
        });
    }

    /// <summary>
    /// Executes an sIRC command repeatedly.
    /// </summary>
    /// <param name="session">The active IRC session.</param>
    /// <param name="command">The command string to execute.</param>
    /// <param name="times">The number of repetitions.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="command"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// May be thrown if command parsing fails or the command cannot be executed.
    /// </exception>
    /// <exception cref="Exception">
    /// Propagates any exceptions thrown by the command execution pipeline.
    /// </exception>
    private async Task RepeatCmdAsync(IIrcSession session, string command, int times = 5)
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

    /// <summary>
    /// Plays a text file line-by-line, sending each line as a message with an optional delay.
    /// </summary>
    /// <param name="session">The active IRC session.</param>
    /// <param name="filePath">The path to the text file.</param>
    /// <param name="cancellationToken">Token used to cancel playback.</param>
    /// <param name="delay">Delay in seconds between lines.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="filePath"/> is null.
    /// </exception>
    /// <exception cref="IOException">
    /// May be thrown when reading the file fails.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// May be thrown if the file cannot be accessed due to permissions.
    /// </exception>
    /// <exception cref="Exception">
    /// Propagates any unexpected exceptions during command execution.
    /// </exception>
    /// <remarks>
    /// Task cancellation exceptions raised during delays are caught internally
    /// and terminate playback gracefully.
    /// </remarks>
    private async Task PlayAsync(IIrcSession session, string filePath, CancellationToken cancellationToken, int delay = 1)
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
