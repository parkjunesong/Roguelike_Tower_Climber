using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    private EquipmentController controller;
    private int index;

    public void Initialize(EquipmentController owner, int slotIndex)
    {
        controller = owner;
        index = slotIndex;
    }

    public void Show(ItemInstance item, bool selected)
    {
        background.color = selected ? new Color(0.35f, 0.55f, 0.8f) : new Color(0.18f, 0.21f, 0.28f);
        icon.sprite = item?.Icon;
        icon.enabled = icon.sprite != null;
        string slotName = UnitEquipment.GetSlotName(index);
        label.text = $"{slotName}\n{(item == null ? "비어 있음" : item.DisplayName)}";
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null) return;
        if (eventData.button == PointerEventData.InputButton.Left) controller.SelectSlot(index);
        else if (eventData.button == PointerEventData.InputButton.Right) controller.ShowContextMenu(index, eventData.position);
    }
}
