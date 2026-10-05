using System;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitEquipment : MonoBehaviour
{
    public const int SlotCount = 6;
    private readonly ItemInstance[] slots = new ItemInstance[SlotCount];
    public event Action Changed;

    public ItemInstance GetItem(int index)
    {
        if (index < 0 || index >= SlotCount) throw new ArgumentOutOfRangeException(nameof(index));
        return slots[index];
    }

    public bool TryEquip(ItemInstance item, InventoryGrid inventory, out string message)
    {
        int source = inventory == null ? -1 : inventory.IndexOf(item);
        if (source < 0 || item.Definition.ActionType != ItemActionType.Equip || item.Definition.EquipmentType == EquipmentType.None)
        {
            message = "인벤토리에 있는 장비를 선택하세요.";
            return false;
        }
        bool weapon = item.Definition.EquipmentType == EquipmentType.Weapon;
        int target = -1;
        for (int i = weapon ? 0 : 1; i < (weapon ? 1 : SlotCount); i++)
            if (slots[i] == null) { target = i; break; }
        if (target < 0)
        {
            message = weapon ? "무기 슬롯이 가득 찼습니다." : "방어구 슬롯이 가득 찼습니다.";
            return false;
        }
        slots[target] = item;
        if (!inventory.TryRemove(source))
        {
            slots[target] = null;
            message = "장비를 인벤토리에서 이동할 수 없습니다.";
            return false;
        }
        Changed?.Invoke();
        message = $"{item.DisplayName} 장착 완료";
        return true;
    }

    public bool TryUnequip(int index, InventoryGrid inventory, out string message)
    {
        if (index < 0 || index >= SlotCount || slots[index] == null || inventory == null)
        {
            message = "해제할 장비를 선택하세요.";
            return false;
        }
        if (inventory.Count == inventory.Capacity)
        {
            message = "인벤토리가 가득 차 장착을 해제할 수 없습니다.";
            return false;
        }
        var item = slots[index];
        slots[index] = null;
        if (!inventory.TryAdd(item))
        {
            slots[index] = item;
            message = "장비를 인벤토리로 이동할 수 없습니다.";
            return false;
        }
        Changed?.Invoke();
        message = $"{item.DisplayName} 장착 해제 완료";
        return true;
    }
}
