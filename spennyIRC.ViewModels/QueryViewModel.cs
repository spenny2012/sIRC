using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels;

public class QueryViewModel : WindowViewModelBase
{
    private IEchoService _echoSvc;

    public QueryViewModel(ISpennyIrcInstance session, string name) : base(session)
    {
        Name = Caption = name;
        _echoSvc = session.Session.EchoService;
    }
    public IEchoService EchoService
    {
        get => _echoSvc;
        set => SetProperty(ref _echoSvc, value);
    }
}
