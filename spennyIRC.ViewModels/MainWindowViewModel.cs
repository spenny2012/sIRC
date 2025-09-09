using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace spennyIRC.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _svc;
    private readonly Dictionary<IIrcSession, IServiceScope> _scopes = [];
#if DEBUG
    private string _title = "sIRC (debuging)";

#else
    private string _title = "sIRC";
#endif
    private ICommand _addServerCommand;
    private IChatWindow _activeContent;
    private ObservableCollection<ServerViewModel> _servers = [];

    public MainWindowViewModel(IServiceProvider svc)
    {
        _svc = svc;
        AddServer();
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public ObservableCollection<ServerViewModel> Servers
    {
        get => _servers;
        set => SetProperty(ref _servers, value);
    }

    public IChatWindow ActiveContent
    {
        get => _activeContent;
        set
        {
            if (value == null) return;

            value.Session.ActiveWindow = value.Name;
            SetProperty(ref _activeContent, value);

        }
    }

    public ICommand AddServerCommand => _addServerCommand ??= new RelayCommand((s) => AddServer(), (o) => true);
    //public ICommand CloseServerCommand => _addServerCommand ??= new RelayCommand((s) => CloseServer(), (o) => true);

    public void AddServer()
    {
        IServiceScope scope = _svc.CreateScope();
        IIrcSession newSession = scope.ServiceProvider.GetRequiredService<IIrcSession>();
        IIrcCommands commands = _svc.GetRequiredService<IIrcCommands>();
        IIrcEvents events = newSession.Events;

        ServerViewModel serverVm = new(newSession, commands);

        List<IIrcRuntimeBinder> eventsToBind =
        [
            new ClientRuntimeBinder(events, newSession.Server, newSession.LocalUser),
            new IalRuntimeBinder(events, newSession.Ial),
            new ViewModelRuntimeBinder(newSession)
        ];

        foreach (IIrcRuntimeBinder evt in eventsToBind)
            evt.Bind();

        Servers.Add(serverVm);
        _scopes.Add(newSession, scope);

        ActiveContent = serverVm;
    }

    public void CloseWindow()
    {
        //Servers.Remove(ActiveContent.Session);
        //_scopes.Add(newSession, scope);
    }
}

