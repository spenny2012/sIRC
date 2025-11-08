using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Commands;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Window;

namespace spennyIRC.ViewModels;

public class QueryViewModel : WindowViewModelBase
{
    private IWindowService _echoSvc;

    public QueryViewModel(IIrcSession session, IIrcCommands commands, string name) : base(session, commands)
    {
        Name = Caption = name;
        _echoSvc = session.WindowService;
    }

    public IWindowService WindowService
    {
        get => _echoSvc;
        set => SetProperty(ref _echoSvc, value);
    }

    protected override void RegisterUISubscriptions()
    {
        WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != Name) return;
            _session.WindowService.Echo(Name, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
        });

        WeakReferenceMessenger.Default.Register<LocalUserNickChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != Name) return;
            ThreadSafeInvoker.Invoke(() => Name = Caption = m.NewNick);
        });


        WeakReferenceMessenger.Default.Register<NickChangedMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != Name) return;
            ThreadSafeInvoker.Invoke(() => Caption = Name = m.NewNick);
        });

        // TODO: test this
        WeakReferenceMessenger.Default.Register<ServerOpenedQueryMessage>(this, (r, m) =>
        {
            if (m.Session != _session || (m.Nick != Name && m.Nick.Equals(Name, StringComparison.OrdinalIgnoreCase))) return;
            Name = m.Nick;
        });
    }
}