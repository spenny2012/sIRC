using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using System.Collections.ObjectModel;

namespace spennyIRC.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Dictionary<IIrcSession, IServiceScope> _scopes = [];
    private readonly IServiceProvider _svc;
    private IChatWindow _activeContent;
    private IAsyncRelayCommand _addServerCommand;
    private ObservableCollection<ServerViewModel> _servers = [];
#if DEBUG
    private string _title = "sIRC (debuging)";

#else
    private string _title = "sIRC";
#endif

    public MainWindowViewModel(IServiceProvider svc)
    {
        _svc = svc;
        AddServer();
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

    public IAsyncRelayCommand AddServerCommand => _addServerCommand ??= new AsyncRelayCommand(AddServer);

    public ObservableCollection<ServerViewModel> Servers
    {
        get => _servers;
        set => SetProperty(ref _servers, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    //public ICommand CloseServerCommand => _addServerCommand ??= new RelayCommand((s) => CloseServer(), (o) => true);

    public Task AddServer()
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

        return Task.CompletedTask;
    }

    public void CloseWindow()
    {
        //Servers.Remove(ActiveContent.Session);
        //_scopes.Add(newSession, scope);
    }
}