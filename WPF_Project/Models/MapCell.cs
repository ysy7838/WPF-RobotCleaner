namespace WPF_Project.Models;

public class MapCell
{
    public int Row { get; init; }

    public int Column { get; init; }

    public bool IsWalkable { get; init; } = true;
}
