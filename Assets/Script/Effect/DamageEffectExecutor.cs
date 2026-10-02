using System.Collections.Generic;

public class DamageEffectExecutor : IEffectExecutor
{
    public bool CanExecute(EffectBinding binding)
    {
        return binding != null && binding.EffectDefinition is DamageEffectDefinition;
    }

    public bool TryExecute(EffectExecutionContext context, EffectBinding binding)
    {
        if (context == null || binding == null) return false;
        if (binding.EffectDefinition is not DamageEffectDefinition) return false;

        IReadOnlyList<Unit> targets = context.ResolveTargets(binding.Target);
        if (targets == null) return false;

        foreach (Unit target in targets)
        {
            if (target != null) target.OnDamaged(binding.FinalValue, context.SourceUnit);
        }

        return true;
    }
}