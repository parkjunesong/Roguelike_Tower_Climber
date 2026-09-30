using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite artwork;

    [Header("Effects")]
    [SerializeField] private List<EffectBinding> effects = new();

    // Identity Properties
    public string Id => id;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public Sprite Artwork => artwork;

    // Core Data Properties
    public IReadOnlyList<EffectBinding> Effects => effects;
}