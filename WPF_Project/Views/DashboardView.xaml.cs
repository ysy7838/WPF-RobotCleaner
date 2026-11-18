using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WPF_Project.ViewModels;

namespace WPF_Project.Views;

public partial class DashboardView : UserControl
{
    private readonly DispatcherTimer demoTimer;

    public DashboardView()
    {
        InitializeComponent();
        demoTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        demoTimer.Tick += (_, _) => (DataContext as DashboardViewModel)?.AdvanceDemo();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Preserve a ViewModel supplied by the host, including an inherited DataContext.
        DataContext ??= new DashboardViewModel();
        demoTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        demoTimer.Stop();
        if (DataContext is DashboardViewModel viewModel && viewModel.PauseDemoCommand.CanExecute(null))
            viewModel.PauseDemoCommand.Execute(null);
    }
}
