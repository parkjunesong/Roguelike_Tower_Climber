# 시나리오 작성

Create > Game > Data > Scenario / Dialogue로 데이터를 생성합니다.

- Steps에 Dialogue, Battle, Explore를 실행 순서대로 추가하고 해당 데이터를 연결합니다.
- Main의 ScenarioBattleButton에 Scenario Data를 연결합니다. 비어 있으면 기존 Battle Data로 전투에 진입합니다.
- Dialogue는 Main의 Dialogue Root에서 재생합니다. Explore는 Main의 Party Formation Root에서 최소 한 명을 편성한 뒤 탐사 씬으로 이동합니다.
- Battle Scene Name, Explore Scene Name, Return Scene Name은 빌드 목록에 등록된 씬 이름입니다. 기본값은 Battle, explore, Main입니다.
- 전투와 탐사에서 복귀할 때 시나리오의 다음 단계를 이어갑니다. 모든 단계가 끝나거나 취소하면 복귀 씬으로 돌아갑니다.
- MainSceneController가 메뉴, 대화, 편성 중 하나만 표시합니다. 대화·편성 전용 씬은 사용하지 않습니다.
- 파티 후보는 Main의 PartyFormationController.availablePlayers에 지정합니다. 시작 무기 후보는 startingWeapons에 지정하고 각 캐릭터의 무기 버튼으로 선택합니다. 편성 화면을 다시 열면 캐릭터·무기 선택을 초기화합니다.

## 대화 연출

- Lines에 화자와 대사를 순서대로 작성합니다.
- 각 대사의 Background, Left, Front, Right와 Bgm은 Keep이면 이전 상태를 유지하고 Set이면 지정한 이미지·음악으로 교체합니다. Set의 참조가 비어 있으면 숨기거나 정지합니다.
- 대화 시작 시 이전 대화의 이미지·음악을 초기화합니다. 첫 대사에 필요한 배경과 캐릭터를 지정하세요.
- 배경 전환, Blackout Duration, Next Line Delay를 지원하며 시간은 실제 경과 시간을 사용합니다.
- 대사는 초당 30자로 표시합니다. 대화창 클릭은 표시 중인 대사 완성 또는 다음 대사 진행입니다.
- AutoButton은 표시 완료 후 1.5초 간격으로 자동 진행합니다.
- MenuButton은 시나리오를 취소합니다. 패널을 닫으면 대화 연출과 BGM도 정지합니다.
