using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Data/Unit")]
public class UnitData : ScriptableObject
{
    [SerializeField] private bool isEnemy = true;

    [Header("Text")]
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private RuntimeAnimatorController artwork;

    [Header("Stats")]
    [SerializeField] public List<StatValue> baseStats;

    [Header("Items")]
    //[SerializeField] public List<ItemDefinition> items;

    public bool IsEnemy => isEnemy;
    public string DisplayName => displayName;
    public string Description => description;
    public RuntimeAnimatorController Artwork => artwork;
    public IReadOnlyList<StatValue> BaseStats => baseStats;
    //public IReadOnlyList<ItemDefinition> Items => items;
}