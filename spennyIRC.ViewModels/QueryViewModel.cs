using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.LocalUser;

namespace spennyIRC.ViewModels;

public class QueryViewModel : WindowViewModelBase
{
    private IEchoService _echoSvc;

    public QueryViewModel(IIrcSession session, IIrcCommands commands, string name) : base(session, commands)
    {
        Name = Caption = name;
        _echoSvc = session.EchoService;
    }

    public IEchoService EchoService
    {
        get => _echoSvc;
        set => SetProperty(ref _echoSvc, value);
    }

    protected override void RegisterUISubscriptions()
    {
        WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != Name) return;
            _session.EchoService.Echo(Name, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
        });

        WeakReferenceMessenger.Default.Register<LocalUserNickChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != Name) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => Name = Caption = m.NewNick);
        });

        //WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        //{
        //    if (m.Session != _session || m.Nick != Name) return;
        //    _session.EchoService.Echo(Name, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
        //});
    }
}