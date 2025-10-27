using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Commands;

namespace spennyIRC.ViewModels;

public abstract class WindowViewModelBase : ViewModelBase, IChatWindow
{
    protected IIrcCommands _commands;
    protected IIrcSession _session;
    protected bool _disposed;
    private string _caption = string.Empty;
    private IAsyncRelayCommand? _executeCommand;
    private bool _isSelected;
    private string _name = string.Empty;
    private string _text = string.Empty;

    public WindowViewModelBase(IIrcSession session, IIrcCommands commands)
    {
        _commands = commands;
        _session = session;
        RegisterUISubscriptions();
    }

    public virtual string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public IAsyncRelayCommand ClearCommand => throw new NotImplementedException();

    public IAsyncRelayCommand ExecuteCommand => _executeCommand ??= new AsyncRelayCommand(DoExecuteCommand);

    public virtual bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public virtual string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public virtual IIrcSession Session
    {
        get => _session;
    }

    public virtual string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        WeakReferenceMessenger.Default.UnregisterAll(this);
        GC.SuppressFinalize(this);
    }

    protected virtual void RegisterUISubscriptions()
    { }

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