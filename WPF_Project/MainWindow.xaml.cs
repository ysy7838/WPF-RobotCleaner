using System.Windows;
using WPF_Project.ViewModels;
using WPF_Project.Views;

namespace WPF_Project;

public partial class MainWindow : Window
{
    private Window? _robotControls;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(OpenRobotControls);
    }

    private void OpenRobotControls(SimulationViewModel simulation)
    {
        if (_robotControls is not null)
        {
            _robotControls.Activate();
            return;
        }

        _robotControls = new Window
        {
            Owner = this,
            Title = "로봇 조작",
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Content = new SimulationView { DataContext = simulation }
        };
        _robotControls.Closed += (_, _) => _robotControls = null;
        _robotControls.Show();
    }
}
