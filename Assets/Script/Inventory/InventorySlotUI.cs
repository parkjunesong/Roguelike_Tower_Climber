using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    private InventoryController controller;
    private int index;

    public void Initialize(InventoryController owner, int slotIndex)
    {
        controller = owner;
        index = slotIndex;
    }

    public void Show(ItemInstance item, bool selected)
    {
        var color = item == null ? new Color(0.18f, 0.21f, 0.28f) : item.Color;
        background.color = selected ? Color.Lerp(color, new Color(0.4f, 0.65f, 1f), 0.5f) : color;
        icon.sprite = item?.Icon;
        icon.enabled = icon.sprite != null;
        label.text = item == null ? $"{index + 1}\n비어 있음" :
            $"{item.DisplayName}\n<size=13>{item.ItemType}</size>";
        var c = background.color;
        label.color = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f > 0.6f ? new Color(0.07f, 0.09f, 0.14f) : Color.white;
    }

    // Keep the slot as the press target instead of the parent ScrollRect.
    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null || eventData.dragging) return;
        if (eventData.button == PointerEventData.InputButton.Left) controller.SelectSlot(index);
        else if (eventData.button == PointerEventData.InputButton.Right) controller.ShowContextMenu(index, eventData.position);
    }

    public void OnBeginDrag(PointerEventData data) => controller?.DragAndDrop?.BeginInventoryDrag(index, gameObject, data);
    public void OnDrag(PointerEventData data) => controller?.DragAndDrop?.Drag(gameObject, data);
    public void OnEndDrag(PointerEventData data) => controller?.DragAndDrop?.EndDrag(gameObject, data);
    private void OnDisable() => controller?.DragAndDrop?.CancelDragFrom(gameObject);
}
