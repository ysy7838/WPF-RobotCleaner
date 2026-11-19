using CommunityToolkit.Mvvm.ComponentModel;
using WPF_Project.Models;

namespace WPF_Project.Services;

public class RobotSimulationService : ObservableObject
{
    private IReadOnlyList<Position> _path = Array.Empty<Position>();
    private int _nextIndex;
    private CancellationTokenSource? _movementCancellation;

    public RobotModel Robot { get; }
    public bool IsRunning => _movementCancellation is not null;
    public bool HasPath => _path.Count > 0;
    public bool HasNextStep => _nextIndex < _path.Count;

    public double ProgressPercent => _path.Count switch
    {
        0 => 0,
        1 => 100,
        _ => 100.0 * (_nextIndex - 1) / (_path.Count - 1)
    };

    public RobotSimulationService(RobotModel robot)
    {
        Robot = robot;
    }

    public void LoadPath(IReadOnlyList<Position> path)
    {
        Stop();
        _path = path.ToArray();
        ResetPosition();
        OnPropertyChanged(nameof(HasPath));
    }

    public bool MoveNext()
    {
        if (IsRunning || !HasNextStep)
        {
            return false;
        }

        Advance();
        return true;
    }

    public async Task StartAsync()
    {
        if (IsRunning || !HasNextStep)
        {
            return;
        }

        using var cancellation = new CancellationTokenSource();
        _movementCancellation = cancellation;
        OnPropertyChanged(nameof(IsRunning));

        try
        {
            while (HasNextStep)
            {
                await Task.Delay(500, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                Advance();
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // 정지, 초기화, 또는 새 경로를 불러올 때 기존 이동을 종료합니다.
        }
        finally
        {
            if (_movementCancellation == cancellation)
            {
                _movementCancellation = null;
                OnPropertyChanged(nameof(IsRunning));
            }
        }
    }

    public void Stop()
    {
        var cancellation = _movementCancellation;
        if (cancellation is null)
        {
            return;
        }

        _movementCancellation = null;
        cancellation.Cancel();
        OnPropertyChanged(nameof(IsRunning));
    }

    public void Reset()
    {
        Stop();
        ResetPosition();
    }

    private void Advance()
    {
        var position = _path[_nextIndex];
        _nextIndex++;
        Robot.SetPosition(position);
        NotifyProgress();
    }

    private void ResetPosition()
    {
        _nextIndex = 1;
        if (HasPath)
        {
            Robot.SetPosition(_path[0]);
        }

        NotifyProgress();
    }

    private void NotifyProgress()
    {
        OnPropertyChanged(nameof(HasNextStep));
        OnPropertyChanged(nameof(ProgressPercent));
    }
}
