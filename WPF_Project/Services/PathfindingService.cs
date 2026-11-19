using WPF_Project.Models;

namespace WPF_Project.Services;

public class PathfindingService
{
    // 장애물 설치 위치

    private static readonly (int Row, int Column)[] Directions =
    [
        (-1, 0),
        (0, 1),
        (1, 0),
        (0, -1)
    ];

    public IReadOnlyList<Position> FindPath(
        MapCell[,] map,
        Position start,
        Position goal)
    {
        ArgumentNullException.ThrowIfNull(map);

        var rowCount = map.GetLength(0);
        var columnCount = map.GetLength(1);

        if (rowCount == 0 || columnCount == 0)
        {
            return [];
        }

        ValidatePosition(start, rowCount, columnCount, nameof(start));
        ValidatePosition(goal, rowCount, columnCount, nameof(goal));

        if (!IsWalkable(map, start) || !IsWalkable(map, goal))
        {
            return [];
        }

        var visited = new bool[rowCount, columnCount];
        var parents = new Position[rowCount, columnCount];
        var queue = new Queue<Position>();

        visited[start.Row, start.Column] = true;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == goal)
            {
                return BuildPath(parents, start, goal);
            }

            foreach (var (rowOffset, columnOffset) in Directions)
            {
                var next = new Position(
                    current.Row + rowOffset,
                    current.Column + columnOffset);

                if (!IsInside(next, rowCount, columnCount)
                    || visited[next.Row, next.Column]
                    || !IsWalkable(map, next))
                {
                    continue;
                }

                visited[next.Row, next.Column] = true;
                parents[next.Row, next.Column] = current;
                queue.Enqueue(next);
            }
        }

        return [];
    }

    private static IReadOnlyList<Position> BuildPath(
        Position[,] parents,
        Position start,
        Position goal)
    {
        var path = new List<Position>(); // 경로 저장 리스트
        var current = goal;

        while (current != start)var path = pathfindingService.FindPath(map, start, goal);

foreach (var position in path)
{
    Console.WriteLine($"Row: {position.Row}, Column: {position.Column}");
}
        {
            path.Add(current);
            current = parents[current.Row, current.Column];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    private static bool IsWalkable(MapCell[,] map, Position position)
    {
        return map[position.Row, position.Column]?.IsWalkable == true;
    }

    private static bool IsInside(Position position, int rowCount, int columnCount)
    {
        return position.Row >= 0
            && position.Row < rowCount
            && position.Column >= 0
            && position.Column < columnCount;
    }

    private static void ValidatePosition(
        Position position,
        int rowCount,
        int columnCount,
        string parameterName)
    {
        if (!IsInside(position, rowCount, columnCount))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                position,
                "맵 범위를 벗어난 위치입니다.");
        }
    }
}
