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

    public bool RequiresSingleTarget => RequiresEnemyTarget || RequiresAllyTarget;
    public bool RequiresEnemyTarget => HasTarget(EffectTarget.SingleEnemy);
    public bool RequiresAllyTarget => HasTarget(EffectTarget.SingleAlly);

    private bool HasTarget(EffectTarget target)
    {
        if (effects == null) return false;
        foreach (var binding in effects)
            if (binding != null && binding.EffectDefinition != null &&
                binding.TriggerType == EffectTriggerType.OnUse && binding.Target == target)
                return true;
        return false;
    }

    public bool CanSelectTarget(Unit source, Unit target)
    {
        var manager = UnitManager.Instance;
        if (manager == null || !manager.IsAvailable(source) || !manager.IsAvailable(target))
            return false;

        bool ally = source.Data.IsEnemy == target.Data.IsEnemy;
        return RequiresSingleTarget && (!RequiresEnemyTarget || !ally) && (!RequiresAllyTarget || ally);
    }
}
