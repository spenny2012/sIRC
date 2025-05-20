using spennyIRC.ViewModels;
using System.Windows;

namespace spennyIRC;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _mainWindowViewmodel;

    public MainWindow(MainWindowViewModel mainWindowViewmodel)
    {
        InitializeComponent();
        DataContext = _mainWindowViewmodel = mainWindowViewmodel;
    }
}
