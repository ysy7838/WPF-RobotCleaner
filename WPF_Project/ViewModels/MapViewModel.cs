using System.Collections.ObjectModel;

namespace WPF_Project.ViewModels
{
    public class MapViewModel
    {
        public ObservableCollection<MapCell> Cells { get; set; }

        public MapViewModel()
        {
            Cells = new ObservableCollection<MapCell>();

            // 7x7 맵 데이터 생성
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Cells.Add(new MapCell
                    {
                        Row = row,
                        Column = col
                    });
                }
            }
        }
    }

    public class MapCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
}