using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EquipmentController : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private Unit[] units;
    [SerializeField] private EquipmentSlotUI[] slots;
    [SerializeField] private TMP_Text unitText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject contextOverlay;
    [SerializeField] private RectTransform contextMenu;
    [SerializeField] private Button dismissButton;
    [SerializeField] private Button unequipButton;
    [Header("Equip target selection")]
    [SerializeField] private GameObject equipTargetOverlay;
    [SerializeField] private Button[] equipTargetButtons;
    [SerializeField] private TMP_Text equipTargetText;
    [SerializeField] private Button cancelEquipTargetButton;
    [SerializeField] private Button dismissEquipTargetButton;

    public Unit SelectedUnit { get; private set; }
    private UnitEquipment equipment;
    private int selectedSlot = -1;
    private int unitIndex;
    private ItemInstance pendingItem;
    public bool IsChoosingEquipTarget => equipTargetOverlay != null && equipTargetOverlay.activeSelf;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(i < UnitEquipment.SlotCount);
            if (i < UnitEquipment.SlotCount) slots[i].Initialize(this, i);
        }
        previousButton.onClick.AddListener(PreviousUnit);
        nextButton.onClick.AddListener(NextUnit);
        dismissButton.onClick.AddListener(CloseContextMenu);
        unequipButton.onClick.AddListener(UnequipSelected);
        for (int i = 0; i < equipTargetButtons.Length; i++)
        {
            int index = i;
            equipTargetButtons[i].onClick.AddListener(() => ConfirmEquipTarget(index));
        }
        cancelEquipTargetButton.onClick.AddListener(CancelEquipTarget);
        dismissEquipTargetButton.onClick.AddListener(CancelEquipTarget);
        CancelEquipTarget();
        CloseContextMenu();
    }

    private void OnEnable()
    {
        inventoryController.ItemEquipRequested += EquipItem;
        UnitClickable.OnUnitClicked += SelectUnit;
        if (equipment != null) equipment.Changed += Refresh;
    }

    private void Start()
    {
        if (SelectedUnit == null && units != null)
            foreach (var unit in units) if (unit != null) { SelectUnit(unit); break; }
        Refresh();
    }

    public void SelectUnit(Unit unit)
    {
        if (equipment != null) equipment.Changed -= Refresh;
        SelectedUnit = unit;
        equipment = unit == null ? null : unit.Equipment;
        selectedSlot = -1;
        if (equipment != null && isActiveAndEnabled) equipment.Changed += Refresh;
        if (units != null) unitIndex = Mathf.Max(0, Array.IndexOf(units, unit));
        Refresh();
    }

    public void SetUnits(Unit[] party)
    {
        units = party;
        SelectUnit(units != null && units.Length > 0 ? units[0] : null);
    }

    public void EquipItem(ItemInstance item)
    {
        if (inventoryController.Inventory.IndexOf(item) < 0) return;
        int slot = UnitEquipment.GetSlotIndex(item.Definition);
        if (slot < 0)
        {
            inventoryController.SetStatus("아이템의 장비 종류와 방어구 부위를 설정하세요.");
            return;
        }
        pendingItem = item;
        equipTargetText.text = $"{item.DisplayName} · {UnitEquipment.GetSlotName(slot)}\n장착할 캐릭터를 선택하세요. 기존 장비는 인벤토리로 돌아갑니다.";
        RefreshEquipTargets();
        equipTargetOverlay.SetActive(true);
    }

    private void RefreshEquipTargets()
    {
        int slot = UnitEquipment.GetSlotIndex(pendingItem?.Definition);
        for (int i = 0; i < equipTargetButtons.Length; i++)
        {
            var unit = units != null && i < units.Length ? units[i] : null;
            var previous = unit != null && slot >= 0 ? unit.Equipment.GetItem(slot) : null;
            equipTargetButtons[i].interactable = unit != null && !unit.IsDead && slot >= 0;
            equipTargetButtons[i].GetComponentInChildren<TMP_Text>().text = unit == null ? "빈 파티 슬롯" :
                $"{i + 1}. {unit.Data.DisplayName}{(unit.IsDead ? " · 전투 불능" : "")}\n현재 장비: {(previous == null ? "없음" : previous.DisplayName)}";
        }
    }

    private void ConfirmEquipTarget(int index)
    {
        var item = pendingItem;
        var unit = units != null && index >= 0 && index < units.Length ? units[index] : null;
        CancelEquipTarget();
        string message;
        if (unit == null || unit.IsDead) message = "장착 가능한 캐릭터를 선택하세요.";
        else if (inventoryController.Inventory.IndexOf(item) < 0) message = "아이템이 인벤토리에 없습니다.";
        else
        {
            SelectUnit(unit);
            equipment.TryEquip(item, inventoryController.Inventory, out message);
        }
        statusText.text = message;
        inventoryController.SetStatus(message);
    }

    public void CancelEquipTarget()
    {
        pendingItem = null;
        if (equipTargetOverlay != null) equipTargetOverlay.SetActive(false);
    }

    public void SelectSlot(int index)
    {
        inventoryController.CloseContextMenu();
        CloseContextMenu();
        selectedSlot = equipment != null && index >= 0 && index < UnitEquipment.SlotCount && equipment.GetItem(index) != null ? index : -1;
        Refresh();
    }

    public void ShowContextMenu(int index, Vector2 screenPosition)
    {
        SelectSlot(index);
        if (selectedSlot < 0) return;
        unequipButton.interactable = inventoryController.Inventory.Count < inventoryController.Inventory.Capacity;
        contextOverlay.SetActive(true);
        var parent = (RectTransform)contextMenu.parent;
        var canvas = parent.GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out var position);
        var rect = parent.rect;
        position.x = Mathf.Clamp(position.x, rect.xMin, rect.xMax - contextMenu.rect.width);
        position.y = Mathf.Clamp(position.y, rect.yMin + contextMenu.rect.height, rect.yMax);
        contextMenu.anchoredPosition = position;
        if (!unequipButton.interactable) statusText.text = "인벤토리가 가득 차 장착 해제가 불가능합니다.";
    }

    public void UnequipSelected()
    {
        CloseContextMenu();
        if (equipment == null) return;
        equipment.TryUnequip(selectedSlot, inventoryController.Inventory, out var message);
        statusText.text = message;
        inventoryController.SetStatus(message);
    }

    public void CloseContextMenu()
    {
        if (contextOverlay != null) contextOverlay.SetActive(false);
    }
    private void PreviousUnit() => CycleUnit(-1);
    private void NextUnit() => CycleUnit(1);

    private void CycleUnit(int direction)
    {
        if (units == null || units.Length == 0) return;
        for (int i = 0; i < units.Length; i++)
        {
            unitIndex = (unitIndex + direction + units.Length) % units.Length;
            if (units[unitIndex] != null) { SelectUnit(units[unitIndex]); return; }
        }
        SelectUnit(null);
    }

    private void Refresh()
    {
        CloseContextMenu();
        unitText.text = SelectedUnit == null ? "선택한 유닛 없음" : SelectedUnit.Data != null ? SelectedUnit.Data.DisplayName : SelectedUnit.name;
        if (equipment == null || selectedSlot >= 0 && equipment.GetItem(selectedSlot) == null) selectedSlot = -1;
        for (int i = 0; i < Mathf.Min(slots.Length, UnitEquipment.SlotCount); i++)
            slots[i].Show(equipment == null ? null : equipment.GetItem(i), i == selectedSlot);
        var item = selectedSlot < 0 ? null : equipment.GetItem(selectedSlot);
        if (item == null)
        {
            detailText.text = "장비 좌클릭: 정보\n장비 우클릭: 장착 해제\n\n인벤토리에서 장착 → 캐릭터 선택\n같은 부위의 장비는 교체됩니다.";
            return;
        }
        var text = new StringBuilder($"{item.DisplayName}\n종류: {item.ItemType}\n\n{item.Description}\n\n제공 능력치");
        bool hasStats = false;
        foreach (UnitStatType type in Enum.GetValues(typeof(UnitStatType)))
        {
            int bonus = item.GetStatBonus(type);
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
        if (SelectedUnit == null && equipment != null) SelectUnit(null);
        if (IsChoosingEquipTarget)
        {
            RefreshEquipTargets();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CancelEquipTarget();
        }
        if (contextOverlay.activeSelf)
        {
            unequipButton.interactable = inventoryController.Inventory.Count < inventoryController.Inventory.Capacity;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CloseContextMenu();
        }
    }

    private void OnDisable()
    {
        inventoryController.ItemEquipRequested -= EquipItem;
        UnitClickable.OnUnitClicked -= SelectUnit;
        if (equipment != null) equipment.Changed -= Refresh;
        CloseContextMenu();
        CancelEquipTarget();
    }

    private void OnDestroy()
    {
        previousButton.onClick.RemoveListener(PreviousUnit);
        nextButton.onClick.RemoveListener(NextUnit);
        dismissButton.onClick.RemoveListener(CloseContextMenu);
        unequipButton.onClick.RemoveListener(UnequipSelected);
        cancelEquipTargetButton.onClick.RemoveListener(CancelEquipTarget);
        dismissEquipTargetButton.onClick.RemoveListener(CancelEquipTarget);
    }
}
