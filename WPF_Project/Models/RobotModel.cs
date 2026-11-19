using CommunityToolkit.Mvvm.ComponentModel;

namespace WPF_Project.Models;

public class RobotModel : ObservableObject
{
    
    private Position? _currentPosition;

    public Position? CurrentPosition
    {
        get => _currentPosition;
        private set => SetProperty(ref _currentPosition, value);
    }


    public void SetPosition(Position position)
    {
        CurrentPosition = position;
    }

   

    
}
