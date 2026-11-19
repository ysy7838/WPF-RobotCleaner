
using WPF_Project.Views;
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

        public MainViewModel()
        {
            // 프로그램 실행 시 MapViewModel 표시
            CurrentViewModel = new MapViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
