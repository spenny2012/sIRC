using System.Diagnostics;
using System.Windows.Threading;

namespace spennyIRC.ViewModels;

public static class ThreadSafeInvoker
{
    /// <summary>
    /// Invokes the specified action on the UI thread if necessary
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="priority">The dispatcher priority (optional)</param>
    /// <returns>True if the action was executed; false if it couldn't be executed</returns>
    public static bool InvokeIfNecessary(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(nameof(action));

        try
        {
            Dispatcher? dispatcher = System.Windows.Application.Current?.Dispatcher;

            if (dispatcher == null)
                return false;

            if (dispatcher.CheckAccess())
            {
                action();
                return true;
            }
            else
            {
                dispatcher.Invoke(action, priority);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in InvokeIfNecessary: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempts to invoke the specified action asynchronously on the UI thread if necessary
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="priority">The dispatcher priority (optional)</param>
    /// <returns>True if the action was queued; false if it couldn't be queued</returns>
    public static bool BeginInvokeIfNecessary(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(nameof(action));

        try
        {
            Dispatcher? dispatcher = System.Windows.Application.Current?.Dispatcher;

            if (dispatcher == null)
                return false;

            if (dispatcher.CheckAccess())
            {
                action();
                return true;
            }
            else
            {
                dispatcher.BeginInvoke(action, priority);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in BeginInvokeIfNeeded: {ex.Message}");
            return false;
        }
    }
}