using System.Collections.Generic;
using UnityEngine;

public enum ItemActionType
{
    None,
    Equip,
    Use
}

[CreateAssetMenu(menuName = "Game/Data/Item")]
public class ItemDefinition : ScriptableObject
{
    [Header("Text")]
    [SerializeField] private string displayName;
    [SerializeField] private string itemType;
    [TextArea(2, 5)]
    [SerializeField] private string description;

    [Header("Interaction")]
    [SerializeField] private ItemActionType actionType;
    [SerializeField] private EquipmentType equipmentType;
    [Tooltip("방어구는 모자, 상의, 하의, 신발 중 부위를 지정해야 합니다.")]
    [SerializeField] private ArmorPart armorPart;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Color color = Color.white;

    [Header("Stat Bonuses")]
    [SerializeField] private List<StatValue> statBonuses = new();

    public string DisplayName => displayName;
    public string ItemType => itemType;
    public string Description => description;
    public ItemActionType ActionType => actionType;
    public EquipmentType EquipmentType => equipmentType;
    public ArmorPart ArmorPart => armorPart;
    public Sprite Icon => icon;
    public Color Color => color;
    public IReadOnlyList<StatValue> StatBonuses => statBonuses;

    // Flat bonuses use the same units as UnitStatus. Repeated entries are added.
    public int GetStatBonus(UnitStatType type)
    {
        int bonus = 0;
        foreach (var stat in statBonuses)
            if (stat != null && stat.type == type) bonus += stat.value;
        return bonus;
    }
}
