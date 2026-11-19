
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Project.Models;
using WPF_Project.Services;
using PathfindingCell = WPF_Project.Models.MapCell;
using RobotPosition = WPF_Project.Models.Position;

namespace WPF_Project.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private const int MapSize = 7;
        private readonly Random random = new Random();

        private readonly PathfindingService _pathfinding = new();
        private readonly RobotSimulationService _simulation;

        public ObservableCollection<MapCell> Cells { get; set; }

        public ICommand ResetCommand { get; }
        public ICommand DashboardCommand { get; }
        public ICommand MoveRobotCommand { get; }

        public MapViewModel(Action<SimulationViewModel> openRobotControls)
        {
            Cells = new ObservableCollection<MapCell>();

            _simulation = new RobotSimulationService(new RobotModel());
            _simulation.Robot.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RobotModel.CurrentPosition))
                {
                    UpdateRobotMarker();
                }
            };

            ResetCommand = new RelayCommand(GenerateMap);

            // 대시보드 화면 연결은 추후 구현
            DashboardCommand = new RelayCommand(() => { });

            var controls = new SimulationViewModel(_simulation);
            MoveRobotCommand = new RelayCommand(() => openRobotControls(controls));

            GenerateMap();
        }

        // 맵 전체 생성
        private void GenerateMap()
        {
            Cells.Clear();

            // 7x7 빈 맵 생성
            for (int row = 0; row < MapSize; row++)
            {
                for (int col = 0; col < MapSize; col++)
                {
                    Cells.Add(new MapCell
                    {
                        Row = row,
                        Column = col,
                        CellType = "Empty",
                        CellColor = Brushes.White
                    });
                }
            }

            // 시작점과 끝점 고정
            Position start = new Position(0, 0);
            Position end = new Position(6, 6);

            // 시작점과 끝점 지정
            SetCell(start, "Start", Brushes.Red);
            SetCell(end, "End", Brushes.Blue);

            // 장애물 개수 랜덤 선택 (3~15개)
            int obstacleCount = random.Next(3, 16);

            // 장애물 랜덤 배치
            int placedCount = 0;

            while (placedCount < obstacleCount)
            {
                int row = random.Next(0, MapSize);
                int col = random.Next(0, MapSize);

                MapCell cell = GetCell(row, col);

                // 시작점, 끝점 및 기존 장애물 위치 제외
                if (cell.CellType != "Empty")
                {
                    continue;
                }

                cell.CellType = "Obstacle";
                cell.CellColor = Brushes.Gray;

                placedCount++;
            }

            var pathfindingMap = new PathfindingCell[MapSize, MapSize];

            foreach (var cell in Cells)
            {
                pathfindingMap[cell.Row, cell.Column] = new PathfindingCell
                {
                    Row = cell.Row,
                    Column = cell.Column,
                    IsWalkable = cell.CellType != "Obstacle"
                };
            }

            var robotStart = new RobotPosition(start.Row, start.Column);
            var robotEnd = new RobotPosition(end.Row, end.Column);
            _simulation.Stop();
            _simulation.LoadPath(_pathfinding.FindPath(pathfindingMap, robotStart, robotEnd));
            _simulation.Robot.SetPosition(robotStart);
            UpdateRobotMarker();
            OnPropertyChanged(nameof(Cells));
        }

        private void UpdateRobotMarker()
        {
            var position = _simulation.Robot.CurrentPosition;

            foreach (var cell in Cells)
            {
                cell.HasRobot = position == new RobotPosition(cell.Row, cell.Column);
            }
        }

        // 특정 좌표의 셀 가져오기
        private MapCell GetCell(int row, int col)
        {
            return Cells[row * MapSize + col];
        }

        // 셀의 종류와 색상 설정
        private void SetCell(
            Position position,
            string type,
            Brush color)
        {
            MapCell cell = GetCell(
                position.Row,
                position.Column);

            cell.CellType = type;
            cell.CellColor = color;
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

    // 맵 셀 데이터
    public class MapCell : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Column { get; set; }

        private bool _hasRobot;

        public bool HasRobot
        {
            get => _hasRobot;
            set
            {
                if (_hasRobot != value)
                {
                    _hasRobot = value;
                    OnPropertyChanged();
                }
            }
        }

        private string cellType = string.Empty;

        public string CellType
        {
            get => cellType;
            set
            {
                cellType = value;
                OnPropertyChanged();
            }
        }

        private Brush cellColor = Brushes.White;

        public Brush CellColor
        {
            get => cellColor;
            set
            {
                cellColor = value;
                OnPropertyChanged();
            }
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

    // 맵 좌표 데이터
    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    // 버튼 명령 처리
    public class RelayCommand : ICommand
    {
        private readonly Action execute;

        public RelayCommand(Action execute)
        {
            this.execute = execute;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            execute();
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
