using System;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitEquipment : MonoBehaviour
{
    public const int SlotCount = 5;
    private readonly ItemInstance[] slots = new ItemInstance[SlotCount];
    public event Action Changed;
    public bool IsClassLocked { get; private set; }
    private UnitClass explorationClass;
    public UnitClass Class => IsClassLocked ? explorationClass : slots[0]?.Definition.WeaponClass ?? UnitClass.None;

    public bool TryLockExplorationClass(out string message)
    {
        message = "";
        if (IsClassLocked) return true;
        if (Class == UnitClass.None)
        {
            message = "탐사 진입 전에 클래스가 지정된 무기를 장착하세요.";
            return false;
        }
        explorationClass = Class;
        IsClassLocked = true;
        Changed?.Invoke();
        return true;
    }

    public void ReleaseExplorationClass()
    {
        if (!IsClassLocked) return;
        IsClassLocked = false;
        explorationClass = UnitClass.None;
        Changed?.Invoke();
    }

    public ItemInstance GetItem(int index)
    {
        if (index < 0 || index >= SlotCount) throw new ArgumentOutOfRangeException(nameof(index));
        return slots[index];
    }

    public int GetStatBonus(UnitStatType type)
    {
        int bonus = 0;
        foreach (var item in slots)
            if (item != null) bonus += item.GetStatBonus(type);
        return bonus;
    }

    private void NotifyChanged()
    {
        GetComponent<Unit>().OnEquipmentChanged();
        Changed?.Invoke();
    }

    public static int GetSlotIndex(ItemDefinition definition)
    {
        if (definition == null || definition.ActionType != ItemActionType.Equip) return -1;
        if (definition.EquipmentType == EquipmentType.Weapon) return 0;
        if (definition.EquipmentType != EquipmentType.Armor) return -1;
        return definition.ArmorPart switch
        {
            ArmorPart.Hat => 1, ArmorPart.Top => 2, ArmorPart.Bottom => 3, ArmorPart.Shoes => 4,
            _ => -1
        };
    }

    public static string GetSlotName(int index) => index switch
    {
        0 => "무기", 1 => "모자", 2 => "상의", 3 => "하의", 4 => "신발", _ => "부위 미지정"
    };

    public bool TryEquip(ItemInstance item, InventoryGrid inventory, out string message)
    {
        int source = inventory == null ? -1 : inventory.IndexOf(item);
        int target = GetSlotIndex(item?.Definition);
        if (source < 0 || target < 0)
        {
            message = "인벤토리에 있는 장비와 올바른 장착 부위를 확인하세요.";
            return false;
        }
        if (target == 0 && item.Definition.WeaponClass == UnitClass.None)
        {
            message = "무기의 종류를 설정하세요.";
            return false;
        }
        if (target == 0 && IsClassLocked && item.Definition.WeaponClass != explorationClass)
        {
            message = $"탐사 중에는 {UnitClassNames.GetName(explorationClass)} 클래스의 무기만 장착할 수 있습니다.";
            return false;
        }
        var previous = slots[target];
        slots[target] = item;
        bool moved = previous == null ? inventory.TryRemove(source) : inventory.TryReplace(source, previous);
        if (!moved)
        {
            slots[target] = previous;
            message = "장비를 인벤토리에서 이동할 수 없습니다.";
            return false;
        }
        NotifyChanged();
        message = previous == null ? $"{item.DisplayName} 장착 완료" :
            $"{GetSlotName(target)} 교체: {previous.DisplayName} → {item.DisplayName}";
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
        NotifyChanged();
        message = $"{item.DisplayName} 장착 해제 완료";
        return true;
    }
}
