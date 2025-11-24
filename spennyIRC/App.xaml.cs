using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;
using spennyIRC.ViewModels;
using System.Diagnostics;
using System.Windows;

// TODO: 1-
/* BUG - Fix nick change bug during connect
 * BUG - Fix chat nick bug where name stays through user disconnect

 * Feature - Add context menus
 * Feature - Ensure disposal of chanel viewmodel means controls are being disposed too
 * Feature - Color windows when new message is added
 * Feature - Use reflection for all commands
 * Feature - Handle channels after disconnect
 * Feature - Come up with an elegant solution for handling the /names bug
 * Feature - Finish channel modes & user status modes in ClientRuntimeBinder and InternalAddressList
 * Feature - ServerViewModel and other viewmodel object disposal
 * Feature - Cleanup:
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
        ICSharpScriptManager scriptManager = _serviceProvider.GetRequiredService<ICSharpScriptManager>();
        IrcCommandsBinder cmdBinder = new(commands, scriptManager);
        ViewModelCommandsBinder modelCmdBinder = new(commands, scriptManager);
        cmdBinder.Bind();
        modelCmdBinder.Bind();

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show("Unhandled Exception", e.ExceptionObject.ToString());
#if DEBUG
        Debug.WriteLine(e.ExceptionObject.ToString());
#endif
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("Unhandled Exception", e.Exception.ToString());
#if DEBUG
        Debug.WriteLine(e.Exception.ToString());
#endif
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.ShowDialog();
        Debug.WriteLine("Closing Application\r\n");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
        services.AddSingleton<IIrcCommands, IrcCommands>();
        services.AddSingleton<ICSharpScriptManager, CSharpScriptManager>();
        services.AddScoped<IIrcInternalAddressList, IrcInternalAddressList>();
        services.AddScoped<IIrcSession, IrcSession>();
        services.AddScoped<IIrcClient, IrcClient>();
        services.AddScoped<IIrcLocalUser, IrcLocalUser>(x =>
        {
            // TODO: replace code belode with fields from settings
            return new IrcLocalUser
            {
                Nick = "chuck",
                Nick2 = "chuck2",
                Ident = MiscHelpers.GenerateRandomString(Random.Shared.Next(3, 9)),
                Realname = MiscHelpers.GenerateRandomString(7)
            };
        });
        services.AddScoped<IIrcEvents, IrcEvents>();
        services.AddScoped<IIrcServer, IrcServerInfo>();
        services.AddScoped<IWindowService, WindowService>();
    }
}