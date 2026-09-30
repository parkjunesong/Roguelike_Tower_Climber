using UnityEngine;

public static class EffectEvaluator
{
    public static bool CanExecute(EffectExecutionContext context, EffectBinding binding)
    {
        if (binding == null || binding.EffectDefinition == null) return false;

        if (binding.MaxTriggersPerTurn > 0 && context.SourceUnit != null)
        {
            var listener = context.SourceUnit.TriggerListener;
            if (listener != null)
            {
                int currentCount = listener.GetTriggerCount(binding);
                if (currentCount >= binding.MaxTriggersPerTurn)
                {
                    Debug.Log($"[{context.SourceUnit.name}] �ߵ� ���� �ʰ�: {binding.EffectDefinition?.name} ({currentCount}/{binding.MaxTriggersPerTurn})");
                    return false;
                }
            }
        }

        if (binding.TriggerType != context.CurrentTrigger)
        {
            return false;
        }

        return CheckCondition(context, binding);
    }

    private static bool CheckCondition(EffectExecutionContext context, EffectBinding binding)
    {
        EffectCondition condition = binding.Condition;
        if (condition == null || condition.Type == ConditionType.Always)
        {
            return true;
        }

        switch (condition.Type)
        {
            case ConditionType.HealthBelowPercent:
                // SelectedTarget�� �켱 �˻�, ������ SourceUnit �˻�
                Unit targetUnit = context.SelectedTarget ?? context.SourceUnit;
                if (targetUnit == null) return false;

                return targetUnit.HPPercent <= condition.Value;

                /*
            case ConditionType.HasBuff:
                Unit checkUnit = context.SelectedTarget ?? context.SourceUnit;
                if (checkUnit == null) return false;

                // EffectTag ��� BuffType �˻�
                return checkUnit.HasBuff(condition.RequiredBuff);
                */

            case ConditionType.TargetCount:
                var targets = context.ResolveTargets(binding.Target);
                return targets != null && targets.Count >= Mathf.RoundToInt(condition.Value);

            case ConditionType.RandomChance:
                return Random.value <= condition.Value;

            default:
                return true;
        }
    }
}