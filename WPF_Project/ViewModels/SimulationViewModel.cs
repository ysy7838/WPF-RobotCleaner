using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF_Project.Services;

namespace WPF_Project.ViewModels;

public partial class SimulationViewModel : ObservableObject
{
    public RobotSimulationService Simulation { get; }

    public SimulationViewModel(RobotSimulationService simulation)
    {
        Simulation = simulation;
        Simulation.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(RobotSimulationService.IsRunning):
                    StopCommand.NotifyCanExecuteChanged();
                    goto case nameof(RobotSimulationService.HasNextStep);
                case nameof(RobotSimulationService.HasNextStep):
                    StartCommand.NotifyCanExecuteChanged();
                    NextCommand.NotifyCanExecuteChanged();
                    break;
                case nameof(RobotSimulationService.HasPath):
                    ResetCommand.NotifyCanExecuteChanged();
                    break;
            }
        };
    }

    private bool CanAdvance() => !Simulation.IsRunning && Simulation.HasNextStep;
    private bool CanStop() => Simulation.IsRunning;
    private bool CanReset() => Simulation.HasPath;

    [RelayCommand(CanExecute = nameof(CanAdvance))]
    private Task StartAsync() => Simulation.StartAsync();

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => Simulation.Stop();

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset() => Simulation.Reset();

    [RelayCommand(CanExecute = nameof(CanAdvance))]
    private void Next() => Simulation.MoveNext();
}
