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
- Background / Left / Front / Right의 Action:
  - Keep: 이전 이미지 유지
  - Set: 지정한 Sprite로 교체
  - Clear: 이미지 숨김
- 첫 대사에서 필요한 이미지를 모두 지정하거나 Clear해서 씬의 초기 표시를 정합니다.
- Sound Effect는 해당 대사 시작 시 한 번 재생합니다.
- Characters Per Second가 0이면 즉시 표시합니다.
- 대화창 클릭: 표시 중이면 대사 전체 표시, 표시 완료 상태이면 다음 대사로 이동합니다.
- AutoButton은 자동 진행을 켜고 끕니다. 대사 표시 완료 후 Auto Delay만큼 기다립니다.
- MenuButton은 시나리오를 종료하고 지정된 복귀 씬으로 돌아갑니다.
- talk 씬을 단독 실행하면 TalkSceneController의 Preview Dialogue가 재생됩니다.

## 선택지: 추후 UI 구현용

DialogueLine.Choices에 표시할 문구와 Event Id를 담을 수 있습니다.
선택지가 있으면 진행을 멈추고 TalkSceneController.ChoiceRequested 이벤트를 발생시킵니다.
향후 선택지 UI에서 SelectChoice(index)를 호출하면 ChoiceSelected(eventId)가 발생하고 다음 대사로 진행합니다.
현재 선택지 UI와 분기 실행은 구현하지 않았으므로 실제 시나리오에서는 Choices를 비워 두세요.

## 변경된 연결

talk 씬의 누락된 대화 컴포넌트를 TalkSceneController로 교체했습니다.
대화창, 화자, 배경, 세 위치 일러스트, 효과음 AudioSource, 자동 및 메뉴 버튼을 연결했습니다.
EventSystem은 프로젝트 입력 설정에 맞는 InputSystemUIInputModule을 사용합니다.
talk 씬을 Build Settings에 추가했습니다.
