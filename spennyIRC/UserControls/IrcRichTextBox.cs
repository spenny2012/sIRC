using spennyIRC.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace spennyIRC.UserControls;

public class IrcRichTextBox : RichTextBox
{
    private ScrollViewer? _scrollViewer;
    private double _lastVerticalOffset;
    private double _lastViewportHeight;
    private double _lastExtentHeight;
    private bool _isResizing;
    private bool _wasAtBottom;
    private DispatcherOperation _pendingResizeAction;

    public IrcRichTextBox() : base()
    {
        Loaded += ChatRichTextBox_Loaded;
    }

    private void ChatRichTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = FindVisualChildBFS<ScrollViewer>(this);
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            SizeChanged += IrcRichTextBox_SizeChanged;
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isResizing || (e.ExtentHeightChange == 0 && e.ViewportHeightChange == 0 && e.VerticalChange == 0))
            return;

        _lastVerticalOffset = e.VerticalOffset;
        _lastViewportHeight = e.ViewportHeight;
        _lastExtentHeight = e.ExtentHeight;
        _wasAtBottom = _lastExtentHeight > _lastViewportHeight &&
                       Math.Abs(_lastVerticalOffset + _lastViewportHeight - _lastExtentHeight) < 1.0;
    }

    private void IrcRichTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_scrollViewer == null) return;

        _isResizing = true;
        _pendingResizeAction?.Abort();

        _pendingResizeAction = Dispatcher.BeginInvoke(() =>
        {
            if (_scrollViewer == null) return;

            double currentExtentHeight = _scrollViewer.ExtentHeight,
                   currentViewportHeight = _scrollViewer.ViewportHeight;

            // If content fits entirely or viewport is invalid, no scrolling needed
            if (currentExtentHeight <= currentViewportHeight || currentViewportHeight <= 0)
            {
                _scrollViewer.ScrollToVerticalOffset(0); // Reset to top
                _isResizing = false;
                return;
            }

            if (_wasAtBottom)
            {
                _scrollViewer.ScrollToEnd();
            }
            else
            {
                // Avoid division by zero and ensure valid scroll range
                double lastScrollableHeight = _lastExtentHeight - _lastViewportHeight;
                double currentScrollableHeight = currentExtentHeight - currentViewportHeight;

                if (lastScrollableHeight > 0 && currentScrollableHeight > 0)
                {
                    double newPosition = (_lastVerticalOffset / lastScrollableHeight) * currentScrollableHeight;
                    newPosition = Math.Clamp(newPosition, 0, currentScrollableHeight);
                    _scrollViewer.ScrollToVerticalOffset(newPosition);
                }
                else
                {
                    _scrollViewer.ScrollToVerticalOffset(0); // Fallback to top
                }
            }
            _isResizing = false;
        }, DispatcherPriority.Background);
    }

    protected override void OnInitialized(EventArgs e)
    {
        this.SetTiledBackground("/spennyIRC;component/Images/blue1.png");
        base.OnInitialized(e);
    }

    private static T? FindVisualChildBFS<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return default;

        Queue<DependencyObject> queue = new();
        queue.Enqueue(parent);

        while (queue.Count > 0)
        {
            DependencyObject current = queue.Dequeue();
            int childrenCount = VisualTreeHelper.GetChildrenCount(current);

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(current, i);
                if (child is T result)
                    return result;
                queue.Enqueue(child);
            }
        }
        return default;
    }
}