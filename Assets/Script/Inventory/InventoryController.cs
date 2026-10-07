using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [SerializeField, Min(1)] private int columns = 5;
    [SerializeField, Min(1)] private int rows = 4;
    [Header("Scene UI")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button sortButton;
    [SerializeField] private GameObject contextOverlay;
    [SerializeField] private RectTransform contextMenu;
    [SerializeField] private Button dismissButton;
    [SerializeField] private Button useButton;
    [SerializeField] private TMP_Text useLabel;
    [SerializeField] private Button discardButton;

    public InventoryGrid Inventory { get; private set; }
    public ItemInstance SelectedItem { get; private set; }
    public CaveParallaxDemo.ExplorationItemDragAndDrop DragAndDrop { get; set; }
    public event Action<ItemInstance> ItemUseRequested;
    public event Action<ItemInstance> ItemEquipRequested;
    public event Action<ItemInstance> ItemDiscarded;
    private List<InventorySlotUI> slots;

    private void Awake()
    {
        Inventory = new InventoryGrid(Mathf.Max(1, columns), Mathf.Max(1, rows));
        Inventory.Changed += Refresh;
        slots = new List<InventorySlotUI>(grid.GetComponentsInChildren<InventorySlotUI>(true));
        while (slots.Count < Inventory.Capacity) slots.Add(Instantiate(slotPrefab, grid.transform));
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(i < Inventory.Capacity);
            slots[i].Initialize(this, i);
        }
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Inventory.Columns;
        ((RectTransform)grid.transform).sizeDelta = new Vector2(
            grid.padding.horizontal + Inventory.Columns * (grid.cellSize.x + grid.spacing.x) - grid.spacing.x,
            grid.padding.vertical + Inventory.Rows * (grid.cellSize.y + grid.spacing.y) - grid.spacing.y);
        sortButton.onClick.AddListener(SortByType);
        dismissButton.onClick.AddListener(CloseContextMenu);
        useButton.onClick.AddListener(UseSelectedItem);
        discardButton.onClick.AddListener(DiscardSelectedItem);
        CloseContextMenu();
        Refresh();
    }

    public bool AddItem(ItemDefinition definition) => definition != null && AddItem(new ItemInstance(definition));

    public bool AddItem(ItemInstance item)
    {
        bool added = Inventory.TryAdd(item);
        statusText.text = added ? $"{item.DisplayName} 추가" : "아이템을 추가할 수 없습니다. 용량과 아이템을 확인하세요.";
        return added;
    }

    public void SelectSlot(int index)
    {
        CloseContextMenu();
        SelectedItem = index >= 0 && index < Inventory.Capacity ? Inventory.GetItem(index) : null;
        Refresh();
    }

    public void ShowContextMenu(int index, Vector2 screenPosition)
    {
        if (DragAndDrop != null) return;
        SelectSlot(index);
        if (SelectedItem == null) return;
        var action = SelectedItem.Definition.ActionType;
        useButton.interactable = action != ItemActionType.None;
        useLabel.text = action == ItemActionType.Equip ? "아이템 장착" : "아이템 사용";
        contextOverlay.SetActive(true);
        var parent = (RectTransform)contextMenu.parent;
        var canvas = parent.GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out var position);
        var bounds = parent.rect;
        position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax - contextMenu.rect.width);
        position.y = Mathf.Clamp(position.y, bounds.yMin + contextMenu.rect.height, bounds.yMax);
        contextMenu.anchoredPosition = position;
    }

    public void CloseContextMenu() => contextOverlay.SetActive(false);

    public void UseSelectedItem()
    {
        var item = SelectedItem;
        CloseContextMenu();
        if (Inventory.IndexOf(item) < 0) return;
        switch (item.Definition.ActionType)
        {
            case ItemActionType.Equip:
                statusText.text = $"{item.DisplayName} 장착 요청";
                ItemEquipRequested?.Invoke(item);
                break;
            case ItemActionType.Use:
                ItemUseRequested?.Invoke(item);
                statusText.text = $"{item.DisplayName} 사용 요청";
                break;
        }
    }

    public void DiscardSelectedItem()
    {
        var item = SelectedItem;
        CloseContextMenu();
        int index = Inventory.IndexOf(item);
        if (index < 0) return;
        Inventory.TryRemove(index);
        ItemDiscarded?.Invoke(item);
        statusText.text = $"{item.DisplayName} 버리기 완료";
    }

    public void SortByType()
    {
        Inventory.SortByType();
        statusText.text = "종류별 정렬 완료";
    }

    public void SetStatus(string message) => statusText.text = message;

    private void Refresh()
    {
        CloseContextMenu();
        if (Inventory.IndexOf(SelectedItem) < 0) SelectedItem = null;
        for (int i = 0; i < Inventory.Capacity; i++) slots[i].Show(Inventory.GetItem(i), Inventory.GetItem(i) == SelectedItem && SelectedItem != null);
        capacityText.text = $"{Inventory.Columns} × {Inventory.Rows}  |  보관 {Inventory.Count} / {Inventory.Capacity}";
        if (SelectedItem == null)
        {
            detailText.text = DragAndDrop != null ? "선택한 아이템 없음\n\n좌클릭: 아이템 정보\n장비를 캐릭터 카드에 드래그하여 장착" :
                "선택한 아이템 없음\n\n좌클릭: 아이템 정보\n우클릭: 사용 / 장착 / 버리기";
            return;
        }
        var text = new StringBuilder($"{SelectedItem.DisplayName}\n종류: {SelectedItem.ItemType}");
        if (SelectedItem.Definition.ActionType == ItemActionType.Equip)
            text.Append($"\n장착 부위: {UnitEquipment.GetSlotName(UnitEquipment.GetSlotIndex(SelectedItem.Definition))}");
        if (SelectedItem.Definition.EquipmentType == EquipmentType.Weapon)
            text.Append($"\n무기 클래스: {UnitClassNames.GetName(SelectedItem.Definition.WeaponClass)}");
        text.Append($"\n\n{SelectedItem.Description}\n\n제공 능력치");
        bool hasStats = false;
        foreach (UnitStatType type in Enum.GetValues(typeof(UnitStatType)))
        {
            int bonus = SelectedItem.GetStatBonus(type);
            if (bonus == 0) continue;
            string label = type switch
            {
                UnitStatType.HP => "체력", UnitStatType.AT => "공격력", UnitStatType.DF => "방어력",
                UnitStatType.CR => "치명타 확률", UnitStatType.CD => "치명타 피해량", UnitStatType.Count => "행동 횟수",
                _ => type.ToString()
            };
            text.Append($"\n{label} ({type}): {bonus:+0;-0;0}");
            hasStats = true;
        }
        if (!hasStats) text.Append("\n없음");
        detailText.text = text.ToString();
    }

    private void Update()
    {
        if (contextOverlay.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CloseContextMenu();
    }

    private void OnDestroy()
    {
        if (Inventory != null) Inventory.Changed -= Refresh;
        if (sortButton != null) sortButton.onClick.RemoveListener(SortByType);
        if (dismissButton != null) dismissButton.onClick.RemoveListener(CloseContextMenu);
        if (useButton != null) useButton.onClick.RemoveListener(UseSelectedItem);
        if (discardButton != null) discardButton.onClick.RemoveListener(DiscardSelectedItem);
    }
}
