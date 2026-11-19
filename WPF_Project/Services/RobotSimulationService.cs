using CommunityToolkit.Mvvm.ComponentModel;
using WPF_Project.Models;

namespace WPF_Project.Services;

public class RobotSimulationService : ObservableObject
{
    private readonly RobotModel _robot;
    private IReadOnlyList<Position> _path = Array.Empty<Position>();
    private int _nextIndex;
    private bool _isRunning;
    private bool _isPaused;

    public bool HasNextStep => _nextIndex < _path.Count;

    public double ProgressPercent => _path.Count switch
    {
        0 => 0,
        1 => 100,
        _ => 100.0 * (_nextIndex - 1) / (_path.Count - 1)
    };

    public RobotSimulationService(RobotModel robot)
    {
        _robot = robot;
    }

    public void LoadPath(IReadOnlyList<Position> path)
    {
        if (_isRunning)
        {
            return;
        }

        _path = path.ToArray();
        _nextIndex = 1;

        if (_path.Count > 0)
        {
            _robot.SetPosition(_path[0]);
        }

        OnPropertyChanged(nameof(ProgressPercent));
    }

    public bool MoveNext()
    {
        if (!HasNextStep || _isPaused)
        {
            return false;
        }

        var nextPosition = _path[_nextIndex];
        _nextIndex++;
        _robot.SetPosition(nextPosition);
        OnPropertyChanged(nameof(ProgressPercent));

        return true;
    }

    public async Task StartAsync()
    {
        if (_isRunning || !HasNextStep)
        {
            return;
        }

        using var cancellation = new CancellationTokenSource();
        _movementCancellation = cancellation;
        _isRunning = true;

        try
        {
            while (HasNextStep)
            {
                await Task.Delay(500, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                MoveNext();
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // 초기화 요청으로 이동을 종료합니다.
        }
        finally
        {
            _movementCancellation = null;
            _isPaused = false;
            _isRunning = false;
        }
    }

    public void Reset()
    {
        _movementCancellation?.Cancel();
        _isPaused = false;
        _nextIndex = 1;

        if (_path.Count > 0)
        {
            _robot.SetPosition(_path[0]);
        }

        OnPropertyChanged(nameof(ProgressPercent));
    }

    public void Pause()
    {
        if (_isRunning)
        {
            _isPaused = true;
        }
    }

    public void Resume()
    {
        _isPaused = false;
    }

    private CancellationTokenSource? _movementCancellation;

    public RobotModel Robot => _robot;
}