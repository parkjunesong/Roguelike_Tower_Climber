# 시나리오 작성

Project 창에서 Create > Game > Data > Scenario / Dialogue로 데이터를 생성합니다.
SO를 사용하므로 Sprite, AudioClip, BattleData를 Inspector에서 직접 연결할 수 있습니다.

## 시나리오

- Steps에 Dialogue 또는 Battle을 실행 순서대로 추가합니다.
- 대화가 없으면 Battle만, 전투가 없으면 Dialogue만 넣습니다.
- Dialogue / Battle 필드에는 해당 단계의 데이터를 연결합니다.
- Dialogue Scene Name, Battle Scene Name, Return Scene Name은 Build Settings에 등록된 씬 이름입니다.
- main 씬의 ScenarioBattleButton의 Scenario Data에 연결하면 실행됩니다.
- Scenario Data가 비어 있으면 기존 Battle Data를 사용한 전투 진입을 유지합니다.
- SampleScenario는 대화 → 전투 → 대화 예제입니다. 전투만 실행하려면 Battle 단계만 담은 ScenarioData를 만들거나 기존 Battle Data 연결을 사용합니다.
- 시나리오 전투는 살아 있는 적이 없어지면 다음 웨이브로 진행하며, 마지막 웨이브가 끝나면 다음 단계로 이동합니다.
- 모든 단계가 끝나면 Return Scene Name으로 돌아갑니다. 진행 상태는 메모리에만 유지되며 저장 기능은 없습니다.

## 대화 연출

- Lines에 화자와 대사를 순서대로 작성합니다.
- DialogueData의 Background는 대화 시작 시 한 번 적용합니다. 대사별 Left / Front / Right는 각 대사 시작 시 적용합니다.
- 이미지의 Action:
  - Keep: 이전 이미지 유지
  - Set: 지정한 Sprite로 교체
  - Clear: 이미지 숨김
- Data의 Background와 첫 대사의 캐릭터 이미지를 지정하거나 Clear해서 씬의 초기 표시를 정합니다. 대화 중 배경을 바꾸려면 별도의 DialogueData로 나눕니다.
- Data의 Bgm에 AudioClip을 지정하면 대화 동안 반복 재생하고, 대화 종료 시 정지합니다. 비어 있으면 재생하지 않습니다.
- 대사 표시 속도는 초당 30자로 고정됩니다.
- 대화창 클릭: 표시 중이면 대사 전체 표시, 표시 완료 상태이면 다음 대사로 이동합니다.
- AutoButton은 자동 진행을 켜고 끕니다. 대사 표시 완료 후 1.5초를 기다립니다. 표시 속도와 대기 시간은 TalkSceneController의 상수이므로 Inspector에서 변경할 수 없습니다.
- MenuButton은 시나리오를 종료하고 지정된 복귀 씬으로 돌아갑니다.
- talk 씬을 단독 실행하면 TalkSceneController의 Preview Dialogue가 재생됩니다.

## 변경된 연결

talk 씬의 누락된 대화 컴포넌트를 TalkSceneController로 교체했습니다.
대화창, 화자, 배경, 세 위치 일러스트, BGM AudioSource, 자동 및 메뉴 버튼을 연결했습니다.
EventSystem은 프로젝트 입력 설정에 맞는 InputSystemUIInputModule을 사용합니다.
talk 씬을 Build Settings에 추가했습니다.
