using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace spennyIRC.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _svc;
    private string _title = "sIRC";
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
            value.Session.Session.ActiveWindow = value.Name;
            SetProperty(ref _activeContent, value);
        }
    }

    public ICommand AddServerCommand => _addServerCommand ??= new RelayCommand((s) => AddServer(), (o) => true);

    public void AddServer()
    {
        ISpennyIrcInstance newSession = _svc.GetRequiredService<ISpennyIrcInstance>();
        ServerViewModel serverVm = new(newSession);
        Servers.Add(serverVm);
        ActiveContent = serverVm;
    }
}

