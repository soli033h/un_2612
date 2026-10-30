# un_2612

Unity 6 기반 멀티플레이어 레이싱 게임 프로젝트입니다.
모든 플레이어가 같은 차량 Prefab을 공유하고, 서버가 보내는 seed로 페인트 색상과 성능 수치가 정해집니다.

> **현재 단계:** 싱글 플레이 주행 프로토타입. 차량 한 대를 Terrain에서 조종할 수 있으며, 네트워크 코드는 구조만 잡혀 있습니다.

## 조종법

| 동작 | 키보드 | 게임패드 | Input Action (`Player` 맵) |
|---|---|---|---|
| 가속 | `W` / `↑` | 왼쪽 스틱 위 | `Move` (y+) |
| 브레이크 / 후진 | `S` / `↓` | 왼쪽 스틱 아래 | `Move` (y−) |
| 조향 | `A` `D` / `←` `→` | 왼쪽 스틱 좌우 | `Move` (x) |
| 핸드브레이크 (드리프트) | `Space` | 아래 버튼 (A / ✕) | `Jump` |
| 부스트 | `Left Shift` | 왼쪽 스틱 누르기 | `Sprint` |
| 차량 리셋 | `E` | 위 버튼 (Y / △) | `Interact` |

- 멈춘 상태에서 `S`를 계속 누르면 후진합니다.
- **드리프트:** 속도가 붙은 상태에서 방향키와 `Space`를 함께 누르거나, 코너에서 가속을 유지해 뒤가 미끄러지면 시작됩니다. 드리프트 중에는 방향키가 슬라이드 각도를 조절합니다.
- **부스트:** 게이지가 15% 이상일 때 시작되고, 키를 떼거나 게이지가 바닥나면 끝납니다. 사용하지 않는 동안 다시 충전됩니다.
- 차가 뒤집힌 채 2.5초가 지나면 자동으로 다시 세워집니다.

화면 왼쪽 위에 속도, 차량 상태, 부스트 게이지, 드리프트 각도, seed가 표시됩니다.

## 실행 방법

### 요구 사항

- Unity **6000.6.6f1** (HDRP 17.6)
- 차량 모델 에셋을 `Assets/Assets/Car/`에 직접 넣어야 합니다. 저장소에는 포함되어 있지 않습니다(아래 참고).

### 에디터에서 실행

1. `Assets/Scenes/DebugScene.unity`를 엽니다.
2. Play를 누릅니다.

씬에 따로 설정할 것은 없습니다. 실행되면 [`DebugSceneBootstrap`](Assets/Scripts/Client/Debugging/DebugSceneBootstrap.cs)이 씬에서 차 모델을 찾아 조종 가능한 차로 만들고, 메인 카메라를 붙입니다.

### 실행파일 빌드

- 에디터 메뉴: **Racing → Build Windows Client**
- 결과물: `Builds/Windows/un_2612.exe` (`Builds/`는 git에서 제외)
- 명령줄 빌드(에디터를 닫은 상태):

  ```
  Unity.exe -batchmode -quit -projectPath . -executeMethod Racing.EditorTools.BuildMenu.BuildWindowsClient
  ```

### 직접 씬에 배치하고 싶다면

자동 설정은 씬에 `Player`가 없을 때만 동작합니다. 직접 배치할 때는 아래처럼 붙입니다.

| 오브젝트 | 컴포넌트 | 연결 |
|---|---|---|
| 차량 (`488_PARENT`) | `CarController` | — |
| `Main Camera` | `CarCameraRig` | — |
| 빈 오브젝트 (`Local Player`) | `Player`, `DebugHud`(선택) | Vehicle ← 차량, Camera Rig ← Main Camera |

Rigidbody, WheelCollider, 충돌 박스, `CarPaint`는 실행할 때 자동으로 만들어지므로 미리 붙이지 않습니다.
`Player`의 **Car Seed**를 0이 아닌 값으로 고정하면 같은 차로 반복 테스트할 수 있습니다.

## 진행 상황

### 완료

- [x] 프로젝트 구조와 어셈블리 분리 (Shared / Client / Server / Editor)
- [x] 차량 모델을 실행 중에 물리 차량으로 조립 (LODGroup, Rigidbody, 차체 충돌 박스, WheelCollider)
- [x] 후륜구동 주행 물리: 속도 감응 조향, 트랙션 컨트롤, 앤티롤바, 다운포스, 공중 자세 보정
- [x] 드리프트 시스템 (진입 쉬움 / 각도 제어 / 속도 보존)
- [x] 부스트 게이지
- [x] 차량 물리 상태 판정 (`CarPhysicsState`)
- [x] seed 기반 성능 수치·색상 생성, 포인트 예산 방식으로 밸런스 유지
- [x] 기본 `InputSystem_Actions`의 `Player` 맵으로 입력 처리
- [x] 추적 카메라와 연출 효과 (충돌·노면 진동, 속도·부스트 FOV, 드리프트 기울임)
- [x] 디버그 HUD, Windows 빌드 메뉴
- [x] 네트워크 프로토콜 초안 ([Docs/protocol.md](Docs/protocol.md))

### 예정

- [ ] 실제 주행 감각에 맞춘 튜닝 값 조정
- [ ] 입력 맵 정리: `Jump`/`Sprint` 이름 변경, 게임패드 트리거로 가속·브레이크
- [ ] UDP 전송과 JSON 직렬화 구현 (`com.unity.nuget.newtonsoft-json` 검토)
- [ ] Dedicated Server: 서버 시뮬레이션 루프, 세션 관리, 스냅샷 브로드캐스트
- [ ] 클라이언트 보간과 예측 (`SnapshotInterpolator`, `RemoteCarInput`)
- [ ] 네트워크 스포너 (`DebugSceneBootstrap` 대체), 차량 Prefab Variant
- [ ] 레이스 진행 (카운트다운, 랩, 순위)

## 구조

### 네트워크 설계

```
클라이언트 ──InputCommand (UDP)──▶ 서버 (Unity Dedicated Server, PhysX로 모든 차 계산)
          ◀──WorldSnapshot 등────
```

- 서버도 같은 Unity 프로젝트를 **Dedicated Server**로 빌드해서, 클라이언트와 같은 물리 코드를 실행합니다.
- 클라이언트는 입력만 보내고, 위치와 상태는 서버가 계산해서 보냅니다.
- 서버는 차량마다 `statSeed`만 보냅니다. 성능 수치와 색상은 서버와 클라이언트가 [`DeterministicRandom`](Assets/Scripts/Shared/Core/DeterministicRandom.cs)으로 똑같이 계산합니다.
- 메시지 명세는 [Docs/protocol.md](Docs/protocol.md)를 참고하세요.

### 어셈블리

| 어셈블리 | 폴더 | 포함되는 빌드 | 내용 |
|---|---|---|---|
| `Racing.Shared` | `Assets/Scripts/Shared` | 서버 + 클라이언트 | 차량 모델·수치·물리, 입력 인터페이스, 네트워크 메시지 |
| `Racing.Client` | `Assets/Scripts/Client` | 클라이언트 (`!UNITY_SERVER`) | Input System, 카메라·연출, 페인트, Player, 디버그 도구 |
| `Racing.Server` | `Assets/Scripts/Server` | 서버 + 에디터 | 시뮬레이션, 세션, 레이스 진행 (구조만 있음) |
| `Racing.Editor` | `Assets/Scripts/Editor` | 에디터 | 빌드 메뉴 |

### 폴더

```
Assets/Scripts/
├─ Shared/
│  ├─ Core/          DeterministicRandom
│  ├─ Vehicles/      Car, CarPhysicsState, TireCompound
│  │  ├─ Stats/      CarStats, DriftCharacteristics, BoostCharacteristics, CarStatRanges, CarStatsGenerator
│  │  └─ Dynamics/   CarController, CarRigBuilder, CarRig, Wheel, CarTuning, DriftSystem, BoostSystem, CarStateEvaluator
│  ├─ Inputs/        ICarInput
│  └─ Networking/    Protocol/ (메시지), Transport/ (UDP, JSON)
├─ Client/
│  ├─ Inputs/        PlayerCarInput
│  ├─ Cameras/       CarCameraRig, Effects/ (CameraShake, SpeedFovEffect, DriftTiltEffect)
│  ├─ Vehicles/      CarPaint
│  ├─ Players/       Player
│  ├─ Networking/    GameClient, SnapshotInterpolator, RemoteCarInput
│  └─ Debugging/     DebugSceneBootstrap, DebugHud
├─ Server/           GameServer, PlayerSession, NetworkCarInput, RaceManager
└─ Editor/           BuildMenu
```

### 주요 클래스 관계

```
Player
 ├─ Car              ← new Car(seed): Stats, PaintColor, State
 ├─ ICarInput        ← PlayerCarInput (내 차) / 네트워크·AI 입력으로 교체 가능
 ├─ CarController    ← Car.Stats와 입력으로 Rigidbody·WheelCollider를 움직이고 Car.State를 갱신
 └─ CarCameraRig     ← 차를 따라가며 ICameraEffect 목록을 합성
```

## 차량 성능 수치

차량을 만들 때 `new Car(seed)`가 [`CarStatsGenerator`](Assets/Scripts/Shared/Vehicles/Stats/CarStatsGenerator.cs)로 수치를 정합니다.

- 7개 능력치(최고 속도, 가속, 핸들링, 제동, 접지력, 드리프트, 부스트)의 **합은 모든 차에서 같고**, 각 능력치는 0.2~0.8 범위에서만 움직입니다.
- 가속과 제동은 m/s² 단위라서 차량 무게와 관계없이 비교할 수 있습니다.
- 무게와 타이어 컴파운드는 장단점이 맞바뀝니다.
  - **Soft:** 접지력 +8%, 드리프트 진입 어려움
  - **Hard:** 접지력 −8%, 드리프트 진입 쉬움
  - 무거운 차일수록 드리프트 진입이 어렵습니다.
- 수치 범위는 `Create → Racing → Car Stat Ranges` 에셋을 만들어 `Player`에 지정하면 바꿀 수 있습니다. 지정하지 않으면 코드의 기본값을 씁니다.
- seed가 같으면 항상 같은 차가 나옵니다. **난수를 뽑는 순서를 바꾸면 모든 seed의 결과가 달라지므로** 서버와 클라이언트를 함께 수정해야 합니다.

주행 느낌(서스펜션, 브레이크 배분, 보조 장치 등)은 `CarController`의 **Tuning** 항목에서 조정합니다. 이 값은 모든 차에 공통입니다.

## 차량 에셋

`Assets/Assets/` 폴더는 외부 에셋이라 `.gitignore`로 제외되어 있습니다. 저장소를 clone한 경우:

- `Assets/Assets/Car/`에 차량 모델을 직접 넣어야 합니다. 넣지 않으면 `DebugScene`의 차량이 Missing Prefab으로 표시됩니다.
- 다른 차량 모델을 쓰려면 바퀴 본 이름이 `BONE_WHEEL_FL/FR/RL/RR`이어야 합니다. 캘리퍼 본 `BONE_CALIPER_*`는 선택이고, LOD는 이름 끝의 `_LOD0`, `_LOD1` … 으로 인식합니다.
- 페인트 색상은 이름이 `_PAINT`로 끝나는 렌더러에 적용됩니다.
