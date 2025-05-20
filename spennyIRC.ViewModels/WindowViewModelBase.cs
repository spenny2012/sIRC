using spennyIRC.Core.IRC.Helpers;
using System.Windows.Input;

namespace spennyIRC.ViewModels;


public abstract class WindowViewModelBase(ISpennyIrcInstance _session) : ViewModelBase, IChatWindow
{
    private bool _isSelected;
    private string _text = string.Empty;
    private string _name = string.Empty;
    private string _caption = string.Empty;
    private ICommand? _executeCommand;

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

    public virtual ISpennyIrcInstance Session
    {
        get => _session;
        set => SetProperty(ref _session, value);
    }

    public ICommand ExecuteCommand => _executeCommand ??= new RelayCommand(async (s) => { await DoExecuteCommand(); }, (o) => true);

    private async Task DoExecuteCommand()
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        if (Text[0] != '/')
        {
            if (!Name.Equals("Status"))
                _session.IrcCommands.ExecuteCommand("say", Text, _session.Session).GetAwaiter().GetResult();

            Text = string.Empty;
            return;
        }

        ExtractedCommandInfo commandInfo = Text.ExtractCommandAndParams();
        Text = string.Empty;

        await _session.IrcCommands.ExecuteCommand(commandInfo.ParsedCmd, commandInfo.CmdParameters, _session.Session);
    }
}


