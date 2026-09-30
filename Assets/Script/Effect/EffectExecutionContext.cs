using System;
using System.Collections.Generic;
using System.Linq;

public class EffectExecutionContext
{
    public Unit SourceUnit { get; }
    public Unit SelectedTarget { get; }
    public EffectTriggerType CurrentTrigger { get; }

    public EffectExecutionContext(Unit sourceUnit, Unit selectedTarget = null, EffectTriggerType currentTrigger = EffectTriggerType.OnUse)
    {
        SourceUnit = sourceUnit ?? throw new ArgumentNullException(nameof(sourceUnit));
        SelectedTarget = selectedTarget;
        CurrentTrigger = currentTrigger;
    }

    public IReadOnlyList<Unit> ResolveTargets(EffectTarget targetType)
    {
        UnitManager manager = UnitManager.Instance;
        if (manager == null || SourceUnit == null) return Array.Empty<Unit>();

        return targetType switch
        {
            EffectTarget.Self => new[] { SourceUnit },
            EffectTarget.SingleEnemy => ResolveSingleTarget(manager, false),
            EffectTarget.SingleAlly => ResolveSingleTarget(manager, true),
            EffectTarget.AllAllies => manager.GetAlliesOf(SourceUnit),
            EffectTarget.AllEnemies => manager.GetEnemiesOf(SourceUnit),
            EffectTarget.AllUnits => manager.GetAllUnits(),
            _ => Array.Empty<Unit>()
        };
    }
    private IReadOnlyList<Unit> ResolveSingleTarget(UnitManager manager, bool ally)
    {
        if (SelectedTarget != null)
        {
            return manager.IsAvailable(SelectedTarget) &&
                (SourceUnit.Data.IsEnemy == SelectedTarget.Data.IsEnemy) == ally
                ? new[] { SelectedTarget } : Array.Empty<Unit>();
        }

        if (SourceUnit.Data.IsEnemy)
        {
            Unit playerUnit = (ally ? manager.GetAlliesOf(SourceUnit) : manager.GetEnemiesOf(SourceUnit)).FirstOrDefault();
            return playerUnit != null ? new[] { playerUnit } : Array.Empty<Unit>();
        }

        return Array.Empty<Unit>();
    }
}