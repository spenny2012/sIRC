using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting;
using spennyIRC.ViewModels;
using System.Diagnostics;
using System.Windows;

// TODO: 1-
/* Fix nick change bug during connect
 * Command binder should use reflection
 * Ensure disposal of chanel viewmodel means controls are being disposed too
 * How should scripts deal with different server instances?
 * Color windows when new message is added
 * Use reflection for all commands
 * Handle channels after disconnect
 * Fix potential bug in PART event in ViewModelRuntimeBinder
 * Come up with an elegant solution for handling the /names bug
 * Finish channel modes & user status modes in ClientRuntimeBinder and InternalAddressList
 * ServerViewModel and other viewmodel object disposal
 * Add context menus
 * Cleanup:
        ServerViewModel WeakReferenceManager calls
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
#if DEBUG
        Trace.Listeners.Add(new TextWriterTraceListener($"debug_{DateTime.Now:yyyy-MM-dd}.txt"));
        Trace.AutoFlush = true;
        Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}] sIRC started..");
#endif

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
#if DEBUG
        Debug.WriteLine(e.ExceptionObject.ToString());
#endif
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("ERROR", e.Exception.ToString());
#if DEBUG
        Debug.WriteLine(e.Exception.ToString());
#endif
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.ShowDialog();
#if DEBUG
        Debug.WriteLine("Closing Application\r\n");
#endif
    }

    private static void ConfigureServices(IServiceCollection services)
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
                Nick = "s" + MiscHelpers.GenerateRandomString(6),
                Nick2 = "YourNick2",
                Ident = MiscHelpers.GenerateRandomString(5),
                Realname = MiscHelpers.GenerateRandomString(3)
            };
        });
        services.AddScoped<IIrcEvents, IrcEvents>();
        services.AddScoped<IIrcServer, IrcServerInfo>();
        services.AddScoped<IIrcClientManager, IrcClientManager>();
        services.AddScoped<IWindowService, WindowService>();
    }
}