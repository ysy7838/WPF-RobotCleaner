
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_Project.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private const int MapSize = 7;
        private readonly Random random = new Random();

        public ObservableCollection<MapCell> Cells { get; set; }

        public ICommand ResetCommand { get; }
        public ICommand DashboardCommand { get; }
        public ICommand MoveRobotCommand { get; }

        public MapViewModel()
        {
            Cells = new ObservableCollection<MapCell>();

            ResetCommand = new RelayCommand(GenerateMap);

            // 대시보드 화면 연결은 추후 구현
            DashboardCommand = new RelayCommand(() => { });

            // 로봇 이동 기능은 추후 구현
            MoveRobotCommand = new RelayCommand(() => { });

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

            // 시작점과 끝점 랜덤 선택
            Position start = GetRandomEdgePosition();
            Position end;

            do
            {
                end = GetRandomEdgePosition();
            }
            while (start.Row == end.Row &&
                   start.Column == end.Column);

            // 장애물 개수 랜덤 선택
            int obstacleCount = random.Next(3, 16);

            bool pathFound = false;

            // BFS 경로가 존재하는 맵이 나올 때까지 재생성
            while (!pathFound)
            {
                // 장애물 초기화
                foreach (MapCell cell in Cells)
                {
                    cell.CellType = "Empty";
                    cell.CellColor = Brushes.White;
                }

                // 시작점과 끝점 지정
                SetCell(start, "Start", Brushes.Red);
                SetCell(end, "End", Brushes.Blue);

                // 장애물 랜덤 배치
                int placedCount = 0;

                while (placedCount < obstacleCount)
                {
                    int row = random.Next(0, MapSize);
                    int col = random.Next(0, MapSize);

                    MapCell cell = GetCell(row, col);

                    if (cell.CellType != "Empty")
                    {
                        continue;
                    }

                    cell.CellType = "Obstacle";
                    cell.CellColor = Brushes.Gray;

                    placedCount++;
                }

                // BFS로 도착 가능 여부 확인
                pathFound = IsPathAvailable(start, end);
            }

            OnPropertyChanged(nameof(Cells));
        }

        // 가장자리 좌표 중 랜덤 선택
        private Position GetRandomEdgePosition()
        {
            int side = random.Next(0, 4);
            int position = random.Next(0, MapSize);

            switch (side)
            {
                case 0:
                    return new Position(0, position);

                case 1:
                    return new Position(MapSize - 1, position);

                case 2:
                    return new Position(position, 0);

                default:
                    return new Position(position, MapSize - 1);
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

        // BFS 경로 검증
        private bool IsPathAvailable(
            Position start,
            Position end)
        {
            bool[,] visited = new bool[MapSize, MapSize];

            Queue<Position> queue = new Queue<Position>();

            // 상, 하, 좌, 우 이동
            int[] rowDirections = { -1, 1, 0, 0 };
            int[] colDirections = { 0, 0, -1, 1 };

            queue.Enqueue(start);
            visited[start.Row, start.Column] = true;

            while (queue.Count > 0)
            {
                Position current = queue.Dequeue();

                // 도착점 도달
                if (current.Row == end.Row &&
                    current.Column == end.Column)
                {
                    return true;
                }

                for (int i = 0; i < 4; i++)
                {
                    int nextRow =
                        current.Row + rowDirections[i];

                    int nextCol =
                        current.Column + colDirections[i];

                    // 맵 범위 확인
                    if (nextRow < 0 || nextRow >= MapSize ||
                        nextCol < 0 || nextCol >= MapSize)
                    {
                        continue;
                    }

                    // 방문 여부 확인
                    if (visited[nextRow, nextCol])
                    {
                        continue;
                    }

                    // 장애물 확인
                    if (GetCell(nextRow, nextCol).CellType
                        == "Obstacle")
                    {
                        continue;
                    }

                    visited[nextRow, nextCol] = true;

                    queue.Enqueue(
                        new Position(nextRow, nextCol));
                }
            }

            return false;
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

    // 맵 셀 데이터
    public class MapCell : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Column { get; set; }

        private string cellType;

        public string CellType
        {
            get => cellType;
            set
            {
                cellType = value;
                OnPropertyChanged();
            }
        }

        private Brush cellColor;

        public Brush CellColor
        {
            get => cellColor;
            set
            {
                cellColor = value;
                OnPropertyChanged();
            }
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

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            execute();
        }

        public event EventHandler CanExecuteChanged;
    }
}
