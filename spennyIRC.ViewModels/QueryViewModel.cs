using spennyIRC.Core.IRC;
using spennyIRC.Scripting;

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
}
