
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
        private const int MapRows = 11;
        private const int MapColumns = 16;

        private readonly Random random = new();
        private readonly PathfindingService _pathfinding = new();
        private readonly RobotSimulationService _simulation;

        // TODO(1): 선택된 알고리즘 enum, 알고리즘 선택 명령, planner/factory 의존성을 이 영역에 추가하세요.
        // TODO(5): MapStateService와 SaveMapCommand/LoadPresetCommand 의존성도 이 영역에 추가하세요.
        public ObservableCollection<MapCell> Cells { get; set; }

        public ICommand ResetCommand { get; }
        public ICommand SelectToolCommand { get; }
        public ICommand EditCellCommand { get; }
        public ICommand ToggleEditModeCommand { get; }
        public ICommand ToggleSimulationModeCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand InitializeCommand { get; }
        public ICommand MoveRobotCommand { get; }

        // TODO(1): SelectAlgorithmCommand를 추가하고 선택 변경 시 UpdatePath()를 호출하세요.
        // TODO(5): 저장·불러오기 명령을 추가하고 파일 선택 UI와 MapStateService를 분리하세요.

        private string selectedTool = "Obstacle";
        private bool isEditMode = true;
        private string selectedAlgorithm = "하이브리드 · 추천";

        // TODO(2): Slider와 연결할 SpeedPercent 또는 DelayMilliseconds 속성을 추가하세요.

        public string SelectedTool
        {
            get => selectedTool;
            private set
            {
                if (selectedTool == value)
                {
                    return;
                }

                selectedTool = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode
        {
            get => isEditMode;
            private set
            {
                if (isEditMode == value)
                {
                    return;
                }

                isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSimulationMode));
            }
        }

        public bool IsSimulationMode => !IsEditMode;

        public string SelectedAlgorithm
        {
            get => selectedAlgorithm;
            private set
            {
                if (selectedAlgorithm == value)
                {
                    return;
                }

                selectedAlgorithm = value;
                OnPropertyChanged();
            }
        }

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
            _simulation.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(RobotSimulationService.ProgressPercent)
                    or nameof(RobotSimulationService.HasPath)
                    or nameof(RobotSimulationService.HasNextStep))
                {
                    OnPropertyChanged(nameof(ProgressPercent));
                    OnPropertyChanged(nameof(MoveCount));
                }
            };

            ResetCommand = new RelayCommand(GenerateMap);
            SelectToolCommand = new RelayCommand(parameter => SelectTool(parameter as string));
            EditCellCommand = new RelayCommand(parameter => EditCell(parameter as MapCell));
            ToggleEditModeCommand = new RelayCommand(() => IsEditMode = true);
            ToggleSimulationModeCommand = new RelayCommand(() => IsEditMode = false);
            StartCommand = new RelayCommand(() => _ = _simulation.StartAsync());
            NextCommand = new RelayCommand(() => _simulation.MoveNext());
            StopCommand = new RelayCommand(() => _simulation.Stop());
            InitializeCommand = new RelayCommand(GenerateMap);

            var controls = new SimulationViewModel(_simulation);
            MoveRobotCommand = new RelayCommand(() => openRobotControls(controls));

            GenerateMap();
        }

        // 맵 전체 생성
        private void GenerateMap()
        {
            Cells.Clear();

            for (int row = 0; row < MapRows; row++)
            {
                for (int col = 0; col < MapColumns; col++)
                {
                    Cells.Add(new MapCell
                    {
                        Row = row,
                        Column = col,
                        CellType = "Empty"
                    });
                }
            }

            RobotPosition start = new(0, 0);
            RobotPosition end = new(MapRows - 1, MapColumns - 1);

            SetCell(start, "Start");
            SetCell(end, "End");

            int obstacleCount = random.Next(3, 16);
            int placedCount = 0;
            while (placedCount < obstacleCount)
            {
                MapCell cell = GetCell(random.Next(MapRows), random.Next(MapColumns));
                if (cell.CellType != "Empty")
                {
                    continue;
                }

                cell.CellType = "Obstacle";
                placedCount++;
            }

            UpdatePath();
            OnPropertyChanged(nameof(Cells));
        }

        private void SelectTool(string? tool)
        {
            if (!string.IsNullOrWhiteSpace(tool))
            {
                SelectedTool = tool;
            }
        }

        private void EditCell(MapCell? cell)
        {
            if (!IsEditMode || cell is null)
            {
                return;
            }

            if (SelectedTool == "Start" || SelectedTool == "End")
            {
                foreach (var existingCell in Cells)
                {
                    if (existingCell.CellType == SelectedTool)
                    {
                        existingCell.CellType = "Empty";
                    }
                }
            }

            cell.CellType = SelectedTool;
            UpdatePath();
        }

        private void UpdatePath()
        {
            // TODO(1)(4): 여기서 선택된 알고리즘 planner를 호출하세요.
            // BFS는 현재 PathfindingService를 재사용하고, 지그재그·나선형·하이브리드는 별도 planner로 분리하세요.
            // TODO(4): 커버리지 알고리즘은 장애물 사이 연결에 BFS를 재사용하고 결과를 _simulation.LoadPath()에 전달하세요.
            MapCell? start = null;
            MapCell? end = null;

            foreach (var cell in Cells)
            {
                if (cell.CellType == "Start") start = cell;
                if (cell.CellType == "End") end = cell;
            }

            if (start is null || end is null)
            {
                _simulation.Stop();
                _simulation.LoadPath(Array.Empty<RobotPosition>());
                foreach (var mapCell in Cells)
                {
                    mapCell.IsOnPath = false;
                }
                return;
            }

            var pathfindingMap = new PathfindingCell[MapRows, MapColumns];
            foreach (var cell in Cells)
            {
                pathfindingMap[cell.Row, cell.Column] = new PathfindingCell
                {
                    Row = cell.Row,
                    Column = cell.Column,
                    IsWalkable = cell.CellType != "Obstacle" && cell.CellType != "Restricted"
                };
            }

            var path = _pathfinding.FindPath(
                pathfindingMap,
                new RobotPosition(start.Row, start.Column),
                new RobotPosition(end.Row, end.Column));
            _simulation.Stop();
            _simulation.LoadPath(path);
            _simulation.Robot.SetPosition(new RobotPosition(start.Row, start.Column));
            foreach (var mapCell in Cells)
            {
                mapCell.IsOnPath = path.Any(position =>
                    position.Row == mapCell.Row && position.Column == mapCell.Column);
            }
            UpdateRobotMarker();
        }

        public double ProgressPercent => _simulation.ProgressPercent;

        public int MoveCount => _simulation.MoveCount;

        // TODO(3): ElapsedTime, Distance, CoveredCellCount, CoveragePercent 등 실시간 통계를 노출하세요.

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
            return Cells[row * MapColumns + col];
        }

        // 셀의 종류와 색상 설정
        private void SetCell(
            RobotPosition position,
            string type)
        {
            MapCell cell = GetCell(
                position.Row,
                position.Column);

            cell.CellType = type;
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

        private bool _isOnPath;

        public bool IsOnPath
        {
            get => _isOnPath;
            set
            {
                if (_isOnPath != value)
                {
                    _isOnPath = value;
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
                OnPropertyChanged(nameof(CellLabel));
            }
        }

        public string CellLabel => CellType switch
        {
            "Start" => "S",
            "End" => "G",
            "Obstacle" => "■",
            "Restricted" => "▧",
            _ => string.Empty
        };

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

    // 버튼 명령 처리
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> execute;

        public RelayCommand(Action execute)
        {
            this.execute = _ => execute();
        }

        public RelayCommand(Action<object?> execute)
        {
            this.execute = execute;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
