using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace WPF_Project.ViewModels;

public enum DashboardRunState
{
    Waiting,
    Running,
    Paused,
    Completed
}

/// <summary>
/// Dashboard-owned presentation state. No dependency on the team's robot or simulation classes.
/// Call ApplyTelemetry on the UI thread when a shared simulation contract is available.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    public const int MaximumLogCount = 200;
    private const int DemoColumns = 10;
    private const int DemoRows = 8;
    private readonly ObservableCollection<string> logs = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Position))]
    private int positionX;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Position))]
    private int positionY;

    [ObservableProperty]
    private int distance;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(CoverageText))]
    private int cleanedCells;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(CoverageText))]
    private int totalCells;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyCanExecuteChangedFor(nameof(StartDemoCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseDemoCommand))]
    private DashboardRunState runState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataSourceText))]
    [NotifyPropertyChangedFor(nameof(DataSourceDescription))]
    [NotifyCanExecuteChangedFor(nameof(StartDemoCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseDemoCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetDemoCommand))]
    private bool isDemoMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastUpdatedText))]
    private DateTime? lastUpdated;

    public DashboardViewModel() : this(enableDemo: true)
    {
    }

    public DashboardViewModel(bool enableDemo)
    {
        Logs = new ReadOnlyObservableCollection<string>(logs);
        IsDemoMode = enableDemo;
        TotalCells = enableDemo ? DemoColumns * DemoRows : 0;
        AddLog(enableDemo ? "대시보드 준비 · 데모 시작을 눌러 주세요." : "대시보드 준비 · 외부 데이터를 기다립니다.");
    }

    public ReadOnlyObservableCollection<string> Logs { get; }
    public string Position => $"({PositionX}, {PositionY})";
    public double Progress => TotalCells > 0 ? (double)CleanedCells / TotalCells * 100 : 0;
    public string CoverageText => $"{CleanedCells:N0} / {TotalCells:N0}칸";
    public string LastUpdatedText => LastUpdated?.ToString("HH:mm:ss") ?? "업데이트 없음";
    public string DataSourceText => IsDemoMode ? "데모 데이터" : "외부 데이터";
    public string DataSourceDescription => IsDemoMode
        ? "10 × 8 예시 공간의 데모입니다. 실제 로봇 데이터가 아닙니다."
        : "연결된 외부 상태를 표시합니다. 데모 제어는 비활성화됩니다.";
    public string StatusText => RunState switch
    {
        DashboardRunState.Running => "청소 중",
        DashboardRunState.Paused => "일시정지",
        DashboardRunState.Completed => "청소 완료",
        _ => "대기"
    };

    private bool CanStartDemo() => IsDemoMode &&
        RunState is DashboardRunState.Waiting or DashboardRunState.Paused;

    private bool CanPauseDemo() => IsDemoMode && RunState == DashboardRunState.Running;
    private bool CanResetDemo() => IsDemoMode;

    [RelayCommand(CanExecute = nameof(CanStartDemo))]
    private void StartDemo()
    {
        // Also guard direct calls; command availability alone does not enforce transitions.
        if (!CanStartDemo()) return;
        bool isResuming = RunState == DashboardRunState.Paused;
        RunState = DashboardRunState.Running;
        LastUpdated = DateTime.Now;
        AddLog(isResuming ? "데모 청소 재개" : "데모 청소 시작");
    }

    [RelayCommand(CanExecute = nameof(CanPauseDemo))]
    private void PauseDemo()
    {
        if (!CanPauseDemo()) return;
        RunState = DashboardRunState.Paused;
        LastUpdated = DateTime.Now;
        AddLog("데모 청소 일시정지");
    }

    [RelayCommand(CanExecute = nameof(CanResetDemo))]
    private void ResetDemo()
    {
        if (!CanResetDemo()) return;
        RunState = DashboardRunState.Waiting;
        PositionX = 0;
        PositionY = 0;
        Distance = 0;
        CleanedCells = 0;
        TotalCells = DemoColumns * DemoRows;
        LastUpdated = DateTime.Now;
        AddLog("데모 초기화 · 위치, 거리, 진행률을 초기화했습니다.");
    }

    [RelayCommand]
    private void ClearLogs() => logs.Clear();

    /// <summary>
    /// Advances one deterministic demo sample. Timing belongs to the view's loaded lifetime.
    /// The starting cell is cleaned without increasing distance; subsequent samples move one cell.
    /// </summary>
    public void AdvanceDemo()
    {
        if (!CanPauseDemo()) return;

        int sample = CleanedCells;
        int row = sample / DemoColumns;
        int column = sample % DemoColumns;
        PositionX = row % 2 == 0 ? column : DemoColumns - 1 - column;
        PositionY = row;
        Distance = sample;
        CleanedCells = sample + 1;
        LastUpdated = DateTime.Now;
        AddLog($"데모 이동 · 위치 {Position} · 이동 {Distance}칸 · 청소 {CoverageText}");

        if (CleanedCells == TotalCells)
        {
            RunState = DashboardRunState.Completed;
            AddLog("데모 청소 완료 · 모든 예시 공간을 청소했습니다.");
        }
    }

    /// <summary>
    /// Integration entry point: supply a complete snapshot on the WPF UI thread.
    /// Valid telemetry permanently switches this instance out of demo mode.
    /// Validate the whole snapshot before changing any observable state.
    /// </summary>
    public void ApplyTelemetry(int x, int y, int distance, int cleanedCells, int totalCells,
        DashboardRunState state, string? message = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfNegative(distance);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCells);
        ArgumentOutOfRangeException.ThrowIfNegative(cleanedCells);
        if (cleanedCells > totalCells)
            throw new ArgumentOutOfRangeException(nameof(cleanedCells), "청소한 칸 수는 전체 칸 수를 초과할 수 없습니다.");
        if (!Enum.IsDefined(state))
            throw new ArgumentOutOfRangeException(nameof(state));
        if (state == DashboardRunState.Completed && (totalCells == 0 || cleanedCells != totalCells))
            throw new ArgumentException("완료 상태는 전체 청소가 끝난 경우에만 지정할 수 있습니다.", nameof(state));

        bool hasChanged = IsDemoMode || PositionX != x || PositionY != y || Distance != distance ||
            CleanedCells != cleanedCells || TotalCells != totalCells || RunState != state;

        IsDemoMode = false;
        PositionX = x;
        PositionY = y;
        Distance = distance;
        CleanedCells = cleanedCells;
        TotalCells = totalCells;
        RunState = state;
        LastUpdated = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(message))
            AddLog(message);
        else if (hasChanged)
            AddLog($"외부 상태 · {StatusText} · 위치 {Position} · 이동 {Distance}칸 · 청소 {CoverageText}");
    }

    private void AddLog(string message)
    {
        logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        if (logs.Count > MaximumLogCount)
            logs.RemoveAt(logs.Count - 1);
    }
}
