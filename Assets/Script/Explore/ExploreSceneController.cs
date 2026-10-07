using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    [DefaultExecutionOrder(-50)]
    public class ExploreSceneController : MonoBehaviour
    {
        [SerializeField] private MapData previewMapData;
        [SerializeField] private SideScrollerPlayer player;
        [SerializeField] private SideScrollerCamera cameraController;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject equipmentUI;
        [SerializeField] private ExplorationPartyHUD partyHUD;
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private Button finishButton;
        [SerializeField] private ExplorationNodeMapUI nodeMapUI;
        [SerializeField] private TMP_Text roomStatus;
        [SerializeField, Min(0.1f)] private float exitDistance = 0.5f;
        [SerializeField] private ExplorationBattleController battleController;
        [SerializeField] private GameObject encounterPanel;
        [SerializeField] private TMP_Text encounterText;
        [SerializeField] private RectTransform choicesRoot;
        [SerializeField] private Button choiceTemplate;

        private readonly HashSet<Vector2Int> visited = new();
        private readonly HashSet<(Vector2Int from, Vector2Int to)> traversedPaths = new();
        private readonly HashSet<Vector2Int> resolved = new();
        private readonly List<Button> choiceButtons = new();
        private bool centerDisplayed;
        private bool exitOffered;
        private Rigidbody2D playerBody;
        private PlayerParty explorationParty;
        public ExplorationMap CurrentMap { get; private set; }
        public MapData CurrentMapData { get; private set; }
        public ExplorationMapLayout Layout { get; private set; }
        public MapNode CurrentNode { get; private set; }
        public bool BossCleared { get; private set; }
        public bool IsInventoryOpen => inventoryUI.activeSelf;
        public bool IsEquipmentOpen => equipmentUI.activeSelf;
        private bool IsWindowOpen => IsInventoryOpen || IsEquipmentOpen;
        public bool CanOpenEquipment => CurrentMap != null && !nodeMapUI.IsOpen &&
            !battleController.IsActive && !encounterPanel.activeSelf;
        public bool IsAtRoomEnd => CurrentMap != null && playerBody.position.x >= player.maximumX - exitDistance;
        private bool CanOpenMap => IsAtRoomEnd;
        public bool CanCompleteExploration => BossCleared && !battleController.IsActive && !encounterPanel.activeSelf && !IsWindowOpen;
        public bool HasVisited(Vector2Int position) => visited.Contains(position);
        public bool HasTraversedPath(Vector2Int from, Vector2Int to) =>
            traversedPaths.Contains((from, to)) || traversedPaths.Contains((to, from));
        public bool IsResolved(Vector2Int position) => resolved.Contains(position);

        private void Start()
        {
            playerBody = player.GetComponent<Rigidbody2D>();
            inventoryUI.SetActive(false);
            equipmentUI.SetActive(false);
            encounterPanel.SetActive(false);
            choiceTemplate.gameObject.SetActive(false);
            var step = ScenarioFlow.CurrentStep;
            CurrentMapData = step != null && step.type == ScenarioStepType.Explore ? step.mapData : previewMapData;
            if (!ExplorationEntry.TryConsume(out var selectedMap, out var party))
            {
                player.enabled = false;
                finishButton.interactable = false;
                enabled = false;
                ExplorationEntry.Begin(CurrentMapData,
                    SceneManager.GetActiveScene().name, ScenarioFlow.Current != null ? ScenarioFlow.Current.returnSceneName : "Main");
                return;
            }
            CurrentMapData = selectedMap;
            explorationParty = party;
            if (CurrentMapData == null || !CurrentMapData.Validate(out _))
            {
                Debug.LogError("Assign a valid MapData in the scenario or scene preview.", this);
                player.enabled = false;
                finishButton.interactable = false;
                return;
            }
            Layout = CurrentMapData.Generate();
            equipmentController.SetUnits(party.Units.ToArray());
            partyHUD.SetUnits(party.Units);
            battleController.Started += OnBattleStarted;
            battleController.Finished += OnBattleFinished;
            nodeMapUI.Initialize(Layout);
            EnterRoom(Layout.GetNode(Layout.Start));
            finishButton.onClick.AddListener(CompleteExploration);
        }

        private void EnterRoom(MapNode node)
        {
            if (CurrentMap != null) Destroy(CurrentMap.gameObject);
            CurrentNode = node;
            visited.Add(node.position);
            exitOffered = false;
            centerDisplayed = false;
            CurrentMap = Instantiate(CurrentMapData.mapPrefab);
            CurrentMap.name = $"Room {node.position.x}, {node.position.y} - {node.DisplayName}";
            player.minimumX = Mathf.Min(CurrentMap.edgePadding, CurrentMap.width * 0.25f);
            player.maximumX = CurrentMap.width - player.minimumX;
            var spawn = new Vector2(player.minimumX, CurrentMap.playerSpawn.y);
            playerBody.linearVelocity = Vector2.zero;
            playerBody.position = spawn;
            player.transform.position = new Vector3(spawn.x, spawn.y, player.transform.position.z);
            cameraController.player = player.transform;
            cameraController.leftEdge = 0;
            cameraController.rightEdge = CurrentMap.width;
            cameraController.mapHeight = CurrentMap.height;
            cameraController.viewportWidth = CurrentMap.viewportWidth;
            cameraController.fitWholeMap = false;
            var camera = cameraController.GetComponent<Camera>();
            camera.orthographicSize = Mathf.Max(CurrentMap.height * 0.5f, CurrentMap.viewportWidth / (2f * camera.aspect));
            camera.transform.position = new Vector3(camera.orthographicSize * camera.aspect, CurrentMap.height * 0.5f, camera.transform.position.z);
            CurrentMap.BindCamera(cameraController.transform);
            roomStatus.text = $"{node.DisplayName} ({node.position.x}, {node.position.y}) | A/D 이동 · I 인벤토리 · E 상호작용\n방 중앙: 이벤트 / 오른쪽 끝: 지도";
            finishButton.interactable = CanCompleteExploration;
            UpdatePlayerControl();
        }

        private void Update()
        {
            if (CurrentMap == null) return;
            finishButton.interactable = CanCompleteExploration;
            if (battleController.IsActive || encounterPanel.activeSelf) return;
            if (!IsWindowOpen && !nodeMapUI.IsOpen)
            {
                if (playerBody.position.x >= CurrentMap.width * 0.5f && !centerDisplayed)
                {
                    centerDisplayed = true;
                    OpenEncounter();
                    if (encounterPanel.activeSelf) return;
                }
                if (!IsAtRoomEnd) exitOffered = false;
                else if (!exitOffered)
                {
                    exitOffered = true;
                    OpenMap();
                }
            }
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (equipmentController.IsChoosingEquipTarget) equipmentController.CancelEquipTarget();
                else if (nodeMapUI.IsOpen) CloseMap();
                else if (IsEquipmentOpen) CloseEquipment();
                else CloseInventory();
            }
            else if (keyboard.iKey.wasPressedThisFrame) ToggleInventory();
            else if ((keyboard.mKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame) && CanOpenMap)
                OpenMap();
            else if (keyboard.eKey.wasPressedThisFrame && !IsWindowOpen && !nodeMapUI.IsOpen &&
                Mathf.Abs(playerBody.position.x - CurrentMap.width * 0.5f) <= 1f) OpenEncounter();
        }

        private void ClearChoices()
        {
            foreach (var button in choiceButtons)
            {
                button.gameObject.SetActive(false);
                Destroy(button.gameObject);
            }
            choiceButtons.Clear();
        }

        private void AddChoice(string label, UnityEngine.Events.UnityAction action)
        {
            var button = Instantiate(choiceTemplate, choicesRoot);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = label;
            button.onClick.AddListener(action);
            choiceButtons.Add(button);
        }

        public void OpenEncounter()
        {
            if (CurrentNode == null || battleController.IsActive || encounterPanel.activeSelf ||
                IsWindowOpen || nodeMapUI.IsOpen || resolved.Contains(CurrentNode.position)) return;
            ClearChoices();
            if (CurrentNode.fixedType == FixedNodeType.Boss)
            {
                encounterText.text = "보스방\n보스를 격파하면 탐사를 완료할 수 있습니다.";
                AddChoice("보스와 싸운다", () => BeginBattle(CurrentMapData.bossBattle));
                AddChoice("준비한다", CloseEncounter);
            }
            else if (CurrentNode.type == MapNodeType.Event)
            {
                var data = CurrentNode.eventData;
                encounterText.text = $"{data.title}\n{data.description}";
                foreach (var choice in data.choices)
                {
                    var selected = choice;
                    AddChoice(selected.label, () => ChooseEvent(selected));
                }
            }
            else
            {
                roomStatus.text = $"{CurrentNode.DisplayName} | 이벤트가 없는 방입니다.";
                return;
            }
            encounterPanel.SetActive(true);
            UpdatePlayerControl();
        }

        private void ChooseEvent(NodeEventChoice choice)
        {
            if (choice.randomEncounter) BeginRandomEncounter();
            else if (choice.battle != null) BeginBattle(choice.battle);
            else
            {
                resolved.Add(CurrentNode.position);
                roomStatus.text = $"{CurrentNode.DisplayName} | {choice.label}";
                CloseEncounter();
            }
        }

        private void CloseEncounter()
        {
            encounterPanel.SetActive(false);
            UpdatePlayerControl();
        }

        private void BeginBattle(BattleData data)
        {
            battleController.Begin(data, PrepareForBattle());
        }

        private void BeginRandomEncounter()
        {
            var enemy = CurrentMapData.GetRandomEncounterEnemy();
            if (enemy != null) battleController.BeginEncounter(enemy, PrepareForBattle());
        }

        private Vector2 PrepareForBattle()
        {
            encounterPanel.SetActive(false);
            cameraController.enabled = false;
            var position = playerBody.position;
            position.x = CurrentMap.width * 0.5f;
            playerBody.position = position;
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            var cameraPosition = cameraController.transform.position;
            cameraPosition.x = position.x;
            cameraPosition.y = CurrentMap.height * 0.4f;
            cameraController.transform.position = cameraPosition;
            player.gameObject.SetActive(false);
            equipmentController.enabled = false;
            return position;
        }

        private void OnBattleStarted()
        {
            CloseInventory();
            CloseEquipment();
            partyHUD.gameObject.SetActive(false);
        }

        private void OnBattleFinished(bool victory)
        {
            partyHUD.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            equipmentController.enabled = true;
            cameraController.SnapToPlayer();
            cameraController.enabled = true;
            if (victory)
            {
                resolved.Add(CurrentNode.position);
                if (CurrentNode.fixedType == FixedNodeType.Boss) BossCleared = true;
            }
            ClearChoices();
            encounterText.text = victory ? (BossCleared ? "보스 격파!\n탐사 완료 버튼으로 탐사를 마칠 수 있습니다." : "전투 승리!\n같은 방에서 탐사를 계속합니다.") :
                "전투 패배\n탐사를 종료합니다.";
            AddChoice(victory ? "계속" : "탐사 종료", victory ? CloseEncounter : EndFailedExploration);
            encounterPanel.SetActive(true);
            UpdatePlayerControl();
        }

        private void EndFailedExploration()
        {
            if (ScenarioFlow.Current != null) ScenarioFlow.Cancel();
            else SceneManager.LoadScene("Main");
        }

        public void OpenMap()
        {
            if (!CanOpenMap || IsWindowOpen || battleController.IsActive || encounterPanel.activeSelf) return;
            nodeMapUI.Show();
            UpdatePlayerControl();
        }

        public void CloseMap()
        {
            nodeMapUI.Hide();
            UpdatePlayerControl();
        }

        public bool MoveToNode(Vector2Int position)
        {
            if (!CanOpenMap || IsWindowOpen || battleController.IsActive || encounterPanel.activeSelf ||
                !nodeMapUI.IsOpen || !Layout.CanMove(CurrentNode.position, position)) return false;
            nodeMapUI.Hide();
            traversedPaths.Add((CurrentNode.position, position));
            EnterRoom(Layout.GetNode(position));
            return true;
        }

        public void ToggleInventory()
        {
            if (CurrentMap == null || nodeMapUI.IsOpen || battleController.IsActive || encounterPanel.activeSelf) return;
            inventoryController.CloseContextMenu();
            equipmentController.CloseContextMenu();
            equipmentController.CancelEquipTarget();
            equipmentUI.SetActive(false);
            inventoryUI.SetActive(!IsInventoryOpen);
            UpdatePlayerControl();
        }

        private void UpdatePlayerControl()
        {
            player.enabled = CurrentMap != null && !IsWindowOpen && !nodeMapUI.IsOpen &&
                !encounterPanel.activeSelf && !battleController.IsActive;
            if (!player.enabled && playerBody != null) playerBody.linearVelocity = Vector2.zero;
        }

        public void CloseInventory()
        {
            equipmentController.CancelEquipTarget();
            inventoryController.CloseContextMenu();
            inventoryUI.SetActive(false);
            UpdatePlayerControl();
        }

        public void OpenEquipment(Unit unit)
        {
            if (!CanOpenEquipment || unit == null) return;
            inventoryController.CloseContextMenu();
            equipmentController.CloseContextMenu();
            equipmentController.CancelEquipTarget();
            inventoryUI.SetActive(false);
            equipmentController.SelectUnit(unit);
            equipmentUI.SetActive(true);
            UpdatePlayerControl();
        }

        public void CloseEquipment()
        {
            equipmentController.CloseContextMenu();
            equipmentUI.SetActive(false);
            UpdatePlayerControl();
        }

        public void CompleteExploration()
        {
            if (!CanCompleteExploration) return;
            if (ScenarioFlow.Current != null) ScenarioFlow.CompleteStep(ScenarioStepType.Explore);
            else SceneManager.LoadScene("Main");
        }

        private void OnDestroy()
        {
            if (explorationParty != null)
                foreach (var unit in explorationParty.Units)
                    if (unit != null) unit.Equipment.ReleaseExplorationClass();
            if (finishButton != null) finishButton.onClick.RemoveListener(CompleteExploration);
            if (battleController != null) battleController.Finished -= OnBattleFinished;
            if (battleController != null) battleController.Started -= OnBattleStarted;
        }
    }
}
