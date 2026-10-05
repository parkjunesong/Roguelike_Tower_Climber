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

    public Unit SelectedUnit { get; private set; }
    private UnitEquipment equipment;
    private int selectedSlot = -1;
    private int unitIndex;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++) slots[i].Initialize(this, i);
        previousButton.onClick.AddListener(PreviousUnit);
        nextButton.onClick.AddListener(NextUnit);
        dismissButton.onClick.AddListener(CloseContextMenu);
        unequipButton.onClick.AddListener(UnequipSelected);
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

    public void EquipItem(ItemInstance item)
    {
        string message;
        if (SelectedUnit == null || SelectedUnit.IsDead) message = "장착할 유닛을 선택하세요.";
        else equipment.TryEquip(item, inventoryController.Inventory, out message);
        statusText.text = message;
        inventoryController.SetStatus(message);
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
        for (int i = 0; i < slots.Length; i++) slots[i].Show(equipment == null ? null : equipment.GetItem(i), i == selectedSlot);
        var item = selectedSlot < 0 ? null : equipment.GetItem(selectedSlot);
        if (item == null)
        {
            detailText.text = "장비 좌클릭: 정보\n장비 우클릭: 장착 해제\n\n인벤토리에서 장착을 선택하면\n현재 유닛의 빈 슬롯으로 이동합니다.";
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
    }

    private void OnDestroy()
    {
        previousButton.onClick.RemoveListener(PreviousUnit);
        nextButton.onClick.RemoveListener(NextUnit);
        dismissButton.onClick.RemoveListener(CloseContextMenu);
        unequipButton.onClick.RemoveListener(UnequipSelected);
    }
}
