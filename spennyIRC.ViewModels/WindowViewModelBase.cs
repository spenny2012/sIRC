using CommunityToolkit.Mvvm.Input;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting;

namespace spennyIRC.ViewModels;

public abstract class WindowViewModelBase(IIrcSession session, IIrcCommands commands) : ViewModelBase, IChatWindow
{
    protected IIrcSession _session = session;
    protected IIrcCommands _commands = commands;
    private bool _isSelected;
    private string _text = string.Empty;
    private string _name = string.Empty;
    private string _caption = string.Empty;
    private IAsyncRelayCommand? _executeCommand;
    private bool _disposed;

    public virtual string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public virtual string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public virtual bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public virtual string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public virtual IIrcSession Session
    {
        get => _session;
    }

    public IAsyncRelayCommand ExecuteCommand => _executeCommand ??= new AsyncRelayCommand(DoExecuteCommand);

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private async Task DoExecuteCommand()
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        if (Text[0] != '/')
        {
            if (!Name.Equals("Status"))
                _commands.ExecuteCommand("say", Text, _session).GetAwaiter().GetResult();
            Text = string.Empty;
            return;
        }

        ExtractedCommandInfo commandInfo = Text.ExtractCommandAndParams();
        Text = string.Empty;
        await _commands.ExecuteCommand(commandInfo.ParsedCmd, commandInfo.CmdParameters, _session);
    }
}