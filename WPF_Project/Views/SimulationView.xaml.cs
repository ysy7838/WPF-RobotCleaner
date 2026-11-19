using System.Windows.Controls;
using WPF_Project.Models;
using WPF_Project.Services;
using WPF_Project.ViewModels;
using WPF_Project.Models;
namespace WPF_Project.Views;

public partial class SimulationView : UserControl
{
    public SimulationView()
    {
        InitializeComponent();
        public RobotModel Robot => _simulation.Robot;

}
}
