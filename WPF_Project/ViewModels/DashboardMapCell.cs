using CommunityToolkit.Mvvm.ComponentModel;

namespace WPF_Project.ViewModels;

/// <summary>Dashboard presentation of a map cell; leaves the team's MapCell unchanged.</summary>
public partial class DashboardMapCell(int row, int column) : ObservableObject
{
    public int Row { get; } = row;
    public int Column { get; } = column;
    public string Coordinates => $"({Column}, {Row})";

    [ObservableProperty]
    private bool isRobot;

    [ObservableProperty]
    private bool isCleaned;
}
