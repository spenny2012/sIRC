using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting;
using spennyIRC.ViewModels;
using System.Windows;
// TODO: 1-
/* How should scripts deal with different server instances?
 * Color windows when new message is added
 * Use reflection for all commands
 * Handle channels after disconnect
 * Add emotes
 * Fix potential bug in PART event in ViewModelRuntimeBinder
 * Come up with an elegant solution for handling the /names bug
 * Finish channel modes & user status modes in ClientRuntimeBinder and InternalAddressList
 * ServerViewModel and other viewmodel object disposal
 * Update nick list during nick changes
 * Add context menus to channels
 * Cleanup:
        ServerViewModel WeakReferenceManager calls
 * Finish implementingIalEventsBinder
 * Add settings class to project, create UI, and plug it in to instances
 */
namespace spennyIRC;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection serviceCollection = new();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        IIrcCommands commands = _serviceProvider.GetRequiredService<IIrcCommands>();
        IrcCommandsBinder cmdBinder = new(commands);
        cmdBinder.Bind();

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show("ERROR", e.ExceptionObject.ToString());
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("ERROR", e.Exception.ToString());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
        services.AddSingleton<IIrcCommands, IrcCommands>();
        services.AddScoped<IIrcInternalAddressList, IrcInternalAddressList>();
        services.AddScoped<IIrcSession, IrcSession>();
        services.AddScoped<IIrcClient, IrcClient>();
        services.AddScoped<IIrcLocalUser, IrcLocalUser>(x =>
        {
            // TODO: replace code belode with fields from settings
            return new IrcLocalUser
            {
                Nick = "s" + MiscHelpers.GenerateRandomString(7),
                Nick2 = "YourNick2",
                Ident = MiscHelpers.GenerateRandomString(3),
                Realname = MiscHelpers.GenerateRandomString(3)
            };
        });
        services.AddScoped<IIrcEvents, IrcEvents>();
        services.AddScoped<IIrcServer, IrcServerInfo>();
        services.AddScoped<IIrcClientManager, IrcClientManager>();
        services.AddScoped<IEchoService, EchoService>();
        services.AddTransient<ISpennyIrcInstance, SpennyIrcInstance>(x =>
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            IServiceProvider sp = scope.ServiceProvider;
            IIrcSession ircSession = sp.GetRequiredService<IIrcSession>();
            IIrcCommands ircCommands = x.GetRequiredService<IIrcCommands>();
            IIrcEvents events = ircSession.Events;
            SpennyIrcInstance spennyIrcSession = new(ircSession, ircCommands, scope);

            List<IIrcRuntimeBinder> eventsToBind =
            [
                new ClientRuntimeBinder(events, ircSession.Server, ircSession.LocalUser),
                new IalRuntimeBinder(events, ircSession.Ial),
                new ViewModelRuntimeBinder(spennyIrcSession)
            ];

            foreach (IIrcRuntimeBinder evt in eventsToBind)
                evt.Bind();

            return spennyIrcSession;
        });
    }
}
