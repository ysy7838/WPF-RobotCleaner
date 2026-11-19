using WPF_Project.Models;

namespace WPF_Project.Services;

public class PathfindingService
{
    // 장애물 설치 위치

    // TODO(4): BFS 외에 지그재그·나선형·하이브리드를 추가할 때는 이 클래스에 모두 넣기보다
    // IPathPlanner와 알고리즘별 planner 클래스로 분리하고, 공통 장애물 연결에는 이 BFS를 재사용하세요.

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

    // TODO(1)(4): 선택된 AlgorithmType에 맞는 planner를 반환하는 factory 또는 dispatcher를 추가하세요.

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
        var path = new List<Position>(); // 경로를 저장할 리스트
        var current = goal;

        while (current != start)
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
