
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_Project.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MapViewModel currentViewModel;

        public MapViewModel CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(Action<SimulationViewModel> openRobotControls)
        {
            // 프로그램 실행 시 MapViewModel 표시
            currentViewModel = new MapViewModel(openRobotControls);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
