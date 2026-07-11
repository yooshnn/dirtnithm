using System.Windows;
using Dirtnithm.App.ViewModels;

namespace Dirtnithm.App.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        _vm.HideRequested += () => Hide();
        _vm.QuitRequested += () => Application.Current.Shutdown();

        Loaded += async (_, _) => await _vm.InitializeAsync();
    }
}