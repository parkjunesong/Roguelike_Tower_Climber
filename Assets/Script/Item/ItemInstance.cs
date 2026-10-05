using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    [SerializeField] private ItemDefinition definition;
    public ItemDefinition Definition => definition;
    public string DisplayName => definition.DisplayName;
    public string ItemType => definition.ItemType;
    public string Description => definition.Description;
    public Color Color => definition.Color;
    public Sprite Icon => definition.Icon;

    public ItemInstance(ItemDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        this.definition = definition;
    }

    public int GetStatBonus(UnitStatType type) => definition.GetStatBonus(type);
}
