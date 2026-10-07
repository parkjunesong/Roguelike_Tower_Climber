using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    public class ExplorationItemDragAndDrop : MonoBehaviour
    {
        [SerializeField] private ExploreSceneController exploration;
        [SerializeField] private InventoryController inventory;
        [SerializeField] private EquipmentController equipment;
        [SerializeField] private RectTransform characterWindow;
        private ItemInstance item;
        private Unit sourceUnit;
        private int sourceSlot;
        private GameObject source;
        private int pointerId;
        private GameObject preview;
        private RectTransform previewRect;

        private void Awake()
        {
            inventory.DragAndDrop = this;
            equipment.DragAndDrop = this;
        }

        private void Start() => inventory.SelectSlot(-1);

        public void BeginInventoryDrag(int index, GameObject sender, PointerEventData data)
        {
            if (!CanBegin(data) || !exploration.IsInventoryOpen) return;
            var candidate = inventory.Inventory.GetItem(index);
            if (UnitEquipment.GetSlotIndex(candidate?.Definition) < 0) return;
            inventory.SelectSlot(index);
            Begin(candidate, null, index, sender, data);
        }

        public void BeginEquipmentDrag(int index, GameObject sender, PointerEventData data)
        {
            if (!CanBegin(data) || !exploration.IsEquipmentOpen || equipment.SelectedUnit == null) return;
            var candidate = equipment.SelectedUnit.Equipment.GetItem(index);
            if (candidate == null) return;
            equipment.SelectSlot(index);
            Begin(candidate, equipment.SelectedUnit, index, sender, data);
        }

        private bool CanBegin(PointerEventData data) => isActiveAndEnabled && exploration.CanOpenEquipment &&
            data.button == PointerEventData.InputButton.Left;

        private void Begin(ItemInstance candidate, Unit unit, int index, GameObject sender, PointerEventData data)
        {
            CancelDrag();
            item = candidate;
            sourceUnit = unit;
            sourceSlot = index;
            source = sender;
            pointerId = data.pointerId;
            data.eligibleForClick = false;
            inventory.CloseContextMenu();
            equipment.CloseContextMenu();
            equipment.CancelEquipTarget();
            CreatePreview(sender);
            MovePreview(data.position);
        }

        public void Drag(GameObject sender, PointerEventData data)
        {
            if (Matches(sender, data)) MovePreview(data.position);
        }

        public void DropOnMember(Unit unit, PointerEventData data)
        {
            if (!Matches(data.pointerDrag, data) || sourceUnit != null || !exploration.IsInventoryOpen ||
                !exploration.CanOpenEquipment) return;
            string message;
            if (unit == null || unit.IsDead) message = "장착 가능한 캐릭터에게 드롭하세요.";
            else unit.Equipment.TryEquip(item, inventory.Inventory, out message);
            equipment.SetStatus(message);
            CancelDrag();
        }

        public void EndDrag(GameObject sender, PointerEventData data)
        {
            if (!Matches(sender, data)) return;
            if (sourceUnit != null && exploration.IsEquipmentOpen && exploration.CanOpenEquipment &&
                equipment.SelectedUnit == sourceUnit && sourceUnit.Equipment.GetItem(sourceSlot) == item)
            {
                var canvas = characterWindow.GetComponentInParent<Canvas>();
                var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                if (!RectTransformUtility.RectangleContainsScreenPoint(characterWindow, data.position, camera))
                {
                    sourceUnit.Equipment.TryUnequip(sourceSlot, inventory.Inventory, out var message);
                    equipment.SetStatus(message);
                }
            }
            CancelDrag();
        }

        private bool Matches(GameObject sender, PointerEventData data) =>
            item != null && source == sender && pointerId == data.pointerId;

        public void CancelDragFrom(GameObject sender)
        {
            if (source == sender) CancelDrag();
        }

        public void CancelDrag()
        {
            item = null;
            sourceUnit = null;
            source = null;
            if (preview != null) Destroy(preview);
            preview = null;
            previewRect = null;
        }

        private void Update()
        {
            if (item == null) return;
            bool valid = source != null && source.activeInHierarchy && exploration.CanOpenEquipment &&
                (sourceUnit == null ? exploration.IsInventoryOpen && inventory.Inventory.IndexOf(item) >= 0 :
                    exploration.IsEquipmentOpen && equipment.SelectedUnit == sourceUnit &&
                    sourceUnit.Equipment.GetItem(sourceSlot) == item);
            if (!valid || Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CancelDrag();
        }

        private void CreatePreview(GameObject sender)
        {
            preview = new GameObject("Dragged Equipment", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            var canvas = preview.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvas.targetDisplay = sender.GetComponentInParent<Canvas>().rootCanvas.targetDisplay;
            var group = preview.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var card = new GameObject("Item", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(preview.transform, false);
            previewRect = card.GetComponent<RectTransform>();
            previewRect.sizeDelta = new Vector2(112f, 132f);
            var background = card.GetComponent<Image>();
            background.color = new Color(0.12f, 0.16f, 0.23f, 0.9f);
            background.raycastTarget = false;
            var imageObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(card.transform, false);
            var image = imageObject.GetComponent<Image>();
            image.sprite = item.Icon;
            image.color = item.Icon != null ? Color.white : item.Color;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(88f, 88f);
            image.rectTransform.anchoredPosition = new Vector2(0f, 16f);
            var labelObject = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(card.transform, false);
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = sender.GetComponentInChildren<TMP_Text>().font;
            label.text = item.DisplayName;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(108f, 36f);
            label.rectTransform.anchoredPosition = new Vector2(0f, -46f);
        }

        private void MovePreview(Vector2 screenPosition)
        {
            if (previewRect == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)preview.transform,
                screenPosition, null, out var position);
            previewRect.anchoredPosition = position;
        }

        private void OnDisable() => CancelDrag();
    }
}
