
# WPF-RobotCleaner
<!-- 프로젝트 실행 화면 추가 -->
<!-- ![RobotCleaner](./images/screenshot.png) -->

**WPF 기반 로봇청소기 시뮬레이터**

LIG넥스원 5기 임베디드 소프트웨어 개발 C# 팀프로젝트입니다.

C#과 WPF를 활용하여 로봇청소기의 이동 및 청소 경로를 시뮬레이션하는 프로그램입니다. BFS(Breadth-First Search) 알고리즘을 이용해 최단 경로를 탐색하며 사용자가 직접 장애물을 배치하여 다양한 환경에서 로봇청소기의 이동 경로를 확인할 수 있습니다.

## Features

- **맵 기반 시뮬레이션**
  - 블록(Block) 단위의 맵 구성
  - 로봇청소기의 위치 및 이동 경로 시각화

- **장애물 배치**
  - 사용자가 원하는 위치에 장애물 직접 배치
  - 아기나 반려동물이 있는 구역 등 진입 제한 구역 설정
  - 장애물 배치에 따른 경로 변화 확인

- **BFS 최단 경로 탐색**
  - BFS 알고리즘을 이용한 최단 경로 탐색
  - 장애물을 고려한 이동 가능 경로 계산
  - 탐색 결과 시각화

- **로봇청소기 이동 시뮬레이션**
  - 탐색된 경로를 따라 로봇청소기 이동
  - 다양한 환경에서의 주행 경로 확인

## Tech Stack

| 구분 | 기술 |
|---|---|
| Language | C# |
| Framework | WPF |
| UI | XAML |
| IDE | Visual Studio |

## Algorithm

### Process

로봇청소기의 시작 위치부터 목적지까지의 최단 경로를 탐색합니다.

1. 로봇청소기의 시작 위치를 큐(Queue)에 삽입합니다.
2. 현재 위치에서 상하좌우 인접한 블록을 탐색합니다.
3. 장애물이 없고 방문하지 않은 블록을 큐에 추가합니다.
4. 목적지에 도달할 때까지 탐색을 반복합니다.
5. 이전 위치 정보를 역추적하여 최단 경로를 구성합니다.
6. 탐색한 경로를 화면에 시각적으로 표시합니다.

## Getting Started

```bash
git clone https://github.com/USERNAME/WPF-RobotCleaner.git
```

1. Visual Studio에서 프로젝트를 실행합니다.
2. 솔루션 파일(`.sln`)을 엽니다.
3. 프로젝트를 빌드합니다.
4. 실행 후 맵과 장애물을 설정하여 시뮬레이션을 진행합니다.

**Robot Cleaner Simulator using C#, WPF, and BFS Algorithm.**
