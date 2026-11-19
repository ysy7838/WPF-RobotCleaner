
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_Project.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // TODO(6)(7): CurrentViewModel 타입을 MapViewModel에 고정하지 말고 공통 ViewModel 또는 object로 확장하세요.
        // TODO(6)(7): DashboardViewModel과 PathViewModel을 생성하고 공유 SimulationState를 전달하세요.
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

            // TODO(6)(7): ShowMapCommand, ShowDashboardCommand, ShowPathCommand를 이 생성자에 연결하세요.
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
