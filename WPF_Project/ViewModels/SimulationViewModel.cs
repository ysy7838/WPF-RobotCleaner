using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF_Project.Services;

namespace WPF_Project.ViewModels;

public partial class SimulationViewModel : ObservableObject
{
    private readonly RobotSimulationService _simulation;

    public SimulationViewModel(RobotSimulationService simulation)
    {
        _simulation = simulation;
    }

    [RelayCommand]
    private Task StartAsync()
    {
        return _simulation.StartAsync();
    }

    [RelayCommand]
    private void Pause()
    {
        _simulation.Pause();
    }

    [RelayCommand]
    private void Resume()
    {
        _simulation.Resume();
    }

    [RelayCommand]
    private void Reset()
    {
        _simulation.Reset();
    }
}