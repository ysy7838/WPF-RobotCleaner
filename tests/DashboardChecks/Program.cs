using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPF_Project;
using WPF_Project.ViewModels;
using WPF_Project.Views;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            CheckDemoTransitions();
            CheckTelemetryAndNotifications();
            CheckInvalidTelemetry();
            CheckLogRetention();
            CheckViewLifetimeAndBindings(args.FirstOrDefault());
            Console.WriteLine("PASS: demo transitions/path, telemetry/notifications, validation, log retention, WPF bindings/lifetime/layout.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void CheckDemoTransitions()
    {
        var vm = new DashboardViewModel();
        Check(vm.RunState == DashboardRunState.Waiting && vm.Progress == 0 && vm.Distance == 0, "Initial state");
        Check(vm.StartDemoCommand.CanExecute(null) && !vm.PauseDemoCommand.CanExecute(null), "Initial commands");
        vm.AdvanceDemo();
        Check(vm.CleanedCells == 0, "No movement before start");
        vm.StartDemoCommand.Execute(null);
        Check(!vm.StartDemoCommand.CanExecute(null) && vm.PauseDemoCommand.CanExecute(null), "Running commands");
        vm.AdvanceDemo();
        Check(vm.CleanedCells == 1 && vm.Distance == 0 && vm.Position == "(0, 0)", "First cell is not a move");
        vm.PauseDemoCommand.Execute(null);
        vm.AdvanceDemo();
        Check(vm.CleanedCells == 1 && vm.RunState == DashboardRunState.Paused, "Pause freezes progress");
        vm.StartDemoCommand.Execute(null);
        for (int i = 1; i < 80; i++)
        {
            int oldX = vm.PositionX, oldY = vm.PositionY;
            vm.AdvanceDemo();
            Check(Math.Abs(vm.PositionX - oldX) + Math.Abs(vm.PositionY - oldY) == 1, "Demo cells must be adjacent");
        }
        Check(vm.RunState == DashboardRunState.Completed && vm.Progress == 100 && vm.Distance == 79 &&
            vm.CleanedCells == 80 && vm.Position == "(0, 7)", "Demo completion");
        vm.AdvanceDemo();
        vm.StartDemoCommand.Execute(null);
        Check(vm.CleanedCells == 80 && !vm.StartDemoCommand.CanExecute(null), "Completed state is stable");
        vm.ResetDemoCommand.Execute(null);
        Check(vm.RunState == DashboardRunState.Waiting && vm.Position == "(0, 0)" &&
            vm.CleanedCells == 0 && vm.Distance == 0 && vm.Progress == 0, "Reset all counters");
        vm.StartDemoCommand.Execute(null);
        vm.AdvanceDemo();
        vm.ResetDemoCommand.Execute(null);
        vm.AdvanceDemo();
        Check(vm.RunState == DashboardRunState.Waiting && vm.CleanedCells == 0, "Reset while running");
    }

    private static void CheckTelemetryAndNotifications()
    {
        var vm = new DashboardViewModel();
        var changed = new HashSet<string?>();
        int commandChanges = 0;
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        vm.StartDemoCommand.CanExecuteChanged += (_, _) => commandChanges++;
        vm.ApplyTelemetry(4, 3, 12, 15, 60, DashboardRunState.Running);
        Check(!vm.IsDemoMode && vm.Position == "(4, 3)" && vm.Distance == 12 && vm.Progress == 25 &&
            vm.CoverageText == "15 / 60칸" && vm.StatusText == "청소 중", "External telemetry");
        Check(new[] { "Position", "Progress", "CoverageText", "StatusText", "DataSourceText", "LastUpdatedText" }
            .All(changed.Contains) && commandChanges > 0, "Dependent notifications");
        Check(!vm.StartDemoCommand.CanExecute(null) && !vm.PauseDemoCommand.CanExecute(null) &&
            !vm.ResetDemoCommand.CanExecute(null), "External mode disables demo commands");
        vm.AdvanceDemo();
        vm.ResetDemoCommand.Execute(null);
        Check(vm.CleanedCells == 15 && !vm.IsDemoMode, "Demo cannot overwrite external state");
        int count = vm.Logs.Count;
        vm.ApplyTelemetry(4, 3, 12, 15, 60, DashboardRunState.Running);
        Check(vm.Logs.Count == count, "Unchanged snapshots do not spam logs");
        vm.ApplyTelemetry(4, 3, 12, 15, 60, DashboardRunState.Paused, "외부 일시정지");
        Check(vm.Logs[0].EndsWith("외부 일시정지") && vm.StatusText == "일시정지", "External event log");
        vm.ApplyTelemetry(0, 0, 0, 0, 0, DashboardRunState.Waiting);
        Check(vm.Progress == 0 && !double.IsNaN(vm.Progress), "Empty map progress");
        vm.ApplyTelemetry(9, 7, 90, 60, 60, DashboardRunState.Completed);
        Check(vm.Progress == 100 && vm.StatusText == "청소 완료", "External completion");
        Check(!new DashboardViewModel(enableDemo: false).IsDemoMode, "External-only constructor");
    }

    private static void CheckInvalidTelemetry()
    {
        var vm = new DashboardViewModel();
        Action[] invalidUpdates =
        [
            () => vm.ApplyTelemetry(-1, 0, 0, 0, 80, DashboardRunState.Waiting),
            () => vm.ApplyTelemetry(0, -1, 0, 0, 80, DashboardRunState.Waiting),
            () => vm.ApplyTelemetry(0, 0, -1, 0, 80, DashboardRunState.Waiting),
            () => vm.ApplyTelemetry(0, 0, 0, -1, 80, DashboardRunState.Waiting),
            () => vm.ApplyTelemetry(0, 0, 0, 0, -1, DashboardRunState.Waiting),
            () => vm.ApplyTelemetry(0, 0, 0, 81, 80, DashboardRunState.Running),
            () => vm.ApplyTelemetry(0, 0, 0, 0, 80, (DashboardRunState)99),
            () => vm.ApplyTelemetry(0, 0, 0, 0, 80, DashboardRunState.Completed),
            () => vm.ApplyTelemetry(0, 0, 0, 0, 0, DashboardRunState.Completed)
        ];
        foreach (Action update in invalidUpdates)
        {
            bool rejected = false;
            try { update(); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected && vm.IsDemoMode && vm.Position == "(0, 0)" && vm.TotalCells == 80 &&
                vm.CleanedCells == 0 && vm.RunState == DashboardRunState.Waiting && vm.LastUpdated is null &&
                vm.Logs.Count == 1, "Invalid snapshot must not partially change state");
        }
    }

    private static void CheckLogRetention()
    {
        var vm = new DashboardViewModel(enableDemo: false);
        for (int i = 0; i < 250; i++)
            vm.ApplyTelemetry(0, 0, i, 0, 80, DashboardRunState.Running, $"sample {i}");
        Check(vm.Logs.Count == DashboardViewModel.MaximumLogCount && vm.Logs[0].EndsWith("sample 249") &&
            vm.Logs[^1].EndsWith("sample 50"), "Bounded newest-first logs");
        vm.ClearLogsCommand.Execute(null);
        Check(vm.Logs.Count == 0, "Clear logs");
    }

    private static void CheckViewLifetimeAndBindings(string? outputDirectory)
    {
        using var bindingErrors = new StringWriter();
        var listener = new TextWriterTraceListener(bindingErrors);
        PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

        var window = new MainWindow();
        Check(window.Content is DashboardView, "Startup window hosts dashboard");
        window.Close();
        var vm = new DashboardViewModel();
        var host = new Grid { DataContext = vm };
        var view = new DashboardView();
        host.Children.Add(view);
        view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        Check(ReferenceEquals(view.DataContext, vm), "Preserve inherited ViewModel");
        vm.StartDemoCommand.Execute(null);
        Pump(TimeSpan.FromMilliseconds(800));
        Check(vm.CleanedCells > 0, "Loaded view timer advances demo");
        view.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        int cleanedBeforeUnload = vm.CleanedCells;
        Pump(TimeSpan.FromMilliseconds(450));
        Check(vm.RunState == DashboardRunState.Paused && vm.CleanedCells == cleanedBeforeUnload,
            "Unloaded view pauses and stops timer");
        view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        vm.StartDemoCommand.Execute(null);
        Pump(TimeSpan.FromMilliseconds(450));
        Check(vm.CleanedCells > cleanedBeforeUnload, "Reload can resume");
        view.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

        vm.ApplyTelemetry(4, 3, 12, 15, 60, DashboardRunState.Running, "외부 데이터 바인딩 확인");
        Render(view, 1000, 900, outputDirectory, "dashboard-wide.png");
        Check(VisualChildren<ProgressBar>(view).Single().Value == 25, "ProgressBar binding");
        Check(VisualChildren<TextBlock>(view).Any(t => t.Text == "(4, 3)"), "Position binding");
        Check(VisualChildren<TextBlock>(view).Any(t => t.Text == "25%"), "Percentage binding");
        Check(VisualChildren<Button>(view).Where(b => b.Content is string s && s.Contains("데모"))
            .All(b => !b.IsEnabled), "External-mode buttons disabled");
        vm.ClearLogsCommand.Execute(null);
        Render(view, 640, 520, outputDirectory, "dashboard-narrow.png");
        Check(VisualChildren<TextBlock>(view).Any(t => t.Text == "기록된 로그가 없습니다." && t.Visibility == Visibility.Visible),
            "Empty log state");

        var fallbackView = new DashboardView();
        fallbackView.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        Check(fallbackView.DataContext is DashboardViewModel, "Standalone ViewModel initialization");
        Render(fallbackView, 1000, 900, outputDirectory, "dashboard-demo.png");
        fallbackView.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        listener.Flush();
        PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
        Check(string.IsNullOrWhiteSpace(bindingErrors.ToString()), $"WPF binding errors: {bindingErrors}");
    }

    private static void Render(FrameworkElement view, int width, int height, string? directory, string filename)
    {
        view.Measure(new Size(width, height));
        view.Arrange(new Rect(0, 0, width, height));
        view.UpdateLayout();
        Pump(TimeSpan.FromMilliseconds(30));
        if (directory is null) return;
        Directory.CreateDirectory(directory);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(directory, filename));
        encoder.Save(stream);
    }

    private static IEnumerable<T> VisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T item) yield return item;
            foreach (T descendant in VisualChildren<T>(child)) yield return descendant;
        }
    }

    private static void Pump(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = duration };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
