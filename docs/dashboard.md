# 대시보드 구현 및 연결 안내

`feature/dashboard`는 `main`의 빈 대시보드를 독립적으로 실행할 수 있게 구현합니다.
공통 `RobotModel`, `RobotSimulationService`, 다른 화면의 ViewModel은 수정하지 않습니다.

## 실행

Windows와 .NET 10 이상 SDK가 필요합니다. 프로젝트의 대상 프레임워크는 기존과 같은
`net10.0-windows`이며 `CommunityToolkit.Mvvm 8.4.2`를 그대로 사용합니다.

```powershell
dotnet build WPF_Project.slnx --configuration Release
dotnet run --project WPF_Project/WPF_Project.csproj --configuration Release
```

빈 시작 창인 `MainWindow.xaml`에는 `DashboardView`를 배치하는 최소 변경을 했습니다.
이후 화면 통합 담당자가 원하는 레이아웃으로 옮겨 사용할 수 있습니다.

## 구현 범위

- 현재 격자 좌표 `(X, Y)`, 누적 이동 거리(칸), 청소한 칸/전체 칸, 진행률 및 진행 막대
- 대기 / 청소 중 / 일시정지 / 청소 완료 상태
- 시각을 포함한 최신순 작업 로그, 최대 200개 보관 및 로그 지우기
- 데모 시작·재개 / 일시정지 / 초기화
- 화면에서 제거되면 데모 타이머 정지 및 일시정지, 재표시 후 수동 재개
- MVVM Toolkit의 `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`를 이용한 바인딩

초기 데이터는 실제 로봇의 상태가 아닌 **10 × 8칸 데모**입니다. 350ms마다 예시 상태를
한 번 갱신합니다. 시작 칸을 청소하는 첫 갱신은 이동 거리에 포함하지 않으므로,
완료 시 80칸 청소 / 79칸 이동 / 좌표 `(0, 7)`입니다. 완료 후에는 초기화해야 다시 시작합니다.
초기화 시 기존 로그는 남으며, 로그 삭제는 별도 버튼으로 처리합니다.

## 팀원 코드와 연결

`DashboardView`는 호스트가 지정하거나 상속한 `DataContext`를 유지합니다.
아무것도 지정하지 않았을 때만 화면 로드 시 기본 데모 ViewModel을 만듭니다.
통합 화면에서는 대시보드에 해당 ViewModel을 명시적으로 연결하세요.

```csharp
var dashboard = new DashboardViewModel(enableDemo: false);
dashboardView.DataContext = dashboard;

// 서비스의 실제 상태를 아래 값으로 매핑합니다. WPF UI 스레드에서 호출해야 합니다.
dashboard.ApplyTelemetry(
    x: 4, y: 3,
    distance: 12,
    cleanedCells: 15,
    totalCells: 60,
    state: DashboardRunState.Running,
    message: "로봇 이동 상태 수신");
```

백그라운드 서비스 이벤트에서는 `dashboardView.Dispatcher.InvokeAsync(...)` 등으로
UI 스레드에 전달한 뒤 `ApplyTelemetry`를 호출하세요. 팀의 시작·정지·완료 상태를
`DashboardRunState`에 매핑하는 작업은 서비스 계약이 완성된 뒤 연결해야 합니다.

유효한 외부 상태를 처음 받으면 해당 인스턴스의 데모 제어가 영구적으로 비활성화됩니다.
데모 타이머가 외부 값을 덮어쓰지 않습니다. 같은 상태를 반복 수신하면 갱신 시각만 변경하고,
명시적인 메시지가 없을 때 중복 로그를 추가하지 않습니다.

음수 좌표·거리·칸 수, 전체보다 많은 청소 칸 수, 정의되지 않은 상태,
청소가 끝나지 않았는데 완료로 표시한 상태는 예외로 거절하며 기존 값을 유지합니다.
전체 0칸의 대기 상태는 허용하고 진행률을 0%로 표시합니다.
생성된 개별 속성을 직접 수정하는 대신 `ApplyTelemetry`를 사용해야 이 검증을 적용할 수 있습니다.

## 검증

외부 테스트 패키지 없이 실행하는 대시보드 전용 검사입니다. 메인 솔루션과 제품에는
테스트 코드를 추가하지 않으며 아래 명령으로 별도 실행합니다.

```powershell
dotnet run --project tests/DashboardChecks/DashboardChecks.csproj --configuration Release
```

검사 범위: 상태 전이와 데모 경로의 인접 이동, 80칸 완료, 중지·초기화,
변경 알림 및 명령 활성화, 외부 상태 전환, 잘못된 입력 거절, 0칸 처리,
로그 보관 한도, WPF 바인딩, `DataContext` 유지, 로드·언로드 시 타이머 수명.
선택적으로 명령 끝에 `-- <출력폴더>`를 붙이면 넓은 화면과 좁은 화면의 PNG도 저장합니다.

## 현재 한계

- 실제 로봇, 지도, 장애물, BFS, 공유 시뮬레이션 서비스에는 아직 연결하지 않았습니다.
- 이동 거리는 물리적 미터가 아닌 격자 사이의 이동 횟수입니다.
- 로그와 상태는 메모리에만 보관하며 재실행하면 사라집니다.
- 자동 검사는 화면을 표시하지 않는 WPF 렌더링과 이벤트 검사입니다. 실제 창에서의
  마우스·키보드 조작, 화면 배율 및 장시간 실행은 별도 수동 검증 대상입니다.
