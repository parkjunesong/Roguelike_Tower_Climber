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

        return targetType switch
        {
            EffectTarget.Self => new[] { SourceUnit },
            EffectTarget.Single => ResolveSingleTarget(manager),
            EffectTarget.AllAllies => manager.GetAlliesOf(SourceUnit),
            EffectTarget.AllEnemies => manager.GetEnemiesOf(SourceUnit),
            EffectTarget.AllUnits => manager.GetAllUnits(),
            _ => Array.Empty<Unit>()
        };
    }
    private IReadOnlyList<Unit> ResolveSingleTarget(UnitManager manager)
    {
        if (SelectedTarget != null)
        {
            return new[] { SelectedTarget };
        }

        // Ÿ���� ����ִµ� �����ڰ� '��(Enemy)'�� ��� -> �� ���忡�� Single ����� ������ �÷��̾�
        if (SourceUnit is Enemy)
        {
            Unit playerUnit = manager.GetEnemiesOf(SourceUnit).FirstOrDefault();
            return new[] { playerUnit };
        }

        return null;
    }
}