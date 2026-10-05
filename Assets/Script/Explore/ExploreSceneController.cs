using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    [DefaultExecutionOrder(-50)]
    public class ExploreSceneController : MonoBehaviour
    {
        [SerializeField] private ExplorationMap previewMapPrefab;
        [SerializeField] private SideScrollerPlayer player;
        [SerializeField] private SideScrollerCamera cameraController;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private Button finishButton;

        public ExplorationMap CurrentMap { get; private set; }
        public bool IsInventoryOpen => inventoryUI.activeSelf;

        private void Start()
        {
            var step = ScenarioFlow.CurrentStep;
            var mapPrefab = step != null && step.type == ScenarioStepType.Explore ? step.mapPrefab : previewMapPrefab;
            if (mapPrefab == null)
            {
                Debug.LogError("Assign an exploration map prefab in the scenario or scene preview.", this);
                player.enabled = false;
                return;
            }
            CurrentMap = Instantiate(mapPrefab);
            CurrentMap.BindCamera(cameraController.transform);
            player.minimumX = CurrentMap.edgePadding;
            player.maximumX = Mathf.Max(player.minimumX, CurrentMap.width - CurrentMap.edgePadding);
            var spawn = CurrentMap.playerSpawn;
            spawn.x = Mathf.Clamp(spawn.x, player.minimumX, player.maximumX);
            player.GetComponent<Rigidbody2D>().position = spawn;
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
            inventoryUI.SetActive(false);
            finishButton.onClick.AddListener(CompleteExploration);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || CurrentMap == null) return;
            if (keyboard.iKey.wasPressedThisFrame) ToggleInventory();
            else if (IsInventoryOpen && keyboard.escapeKey.wasPressedThisFrame) CloseInventory();
        }

        public void ToggleInventory()
        {
            if (CurrentMap == null) return;
            bool open = !IsInventoryOpen;
            inventoryController.CloseContextMenu();
            equipmentController.CloseContextMenu();
            inventoryUI.SetActive(open);
            player.enabled = !open;
            if (open) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        public void CloseInventory()
        {
            if (IsInventoryOpen) ToggleInventory();
        }

        public void CompleteExploration()
        {
            if (ScenarioFlow.Current != null) ScenarioFlow.CompleteStep(ScenarioStepType.Explore);
            else SceneManager.LoadScene("Main");
        }

        private void OnDestroy()
        {
            if (finishButton != null) finishButton.onClick.RemoveListener(CompleteExploration);
        }
    }
}
