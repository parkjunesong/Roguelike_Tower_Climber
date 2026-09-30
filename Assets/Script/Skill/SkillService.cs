
public static class SkillService
{
    public static bool UseSkill(SkillDefinition item, Unit user, Unit target = null)
    {
        if (item == null || user == null) return false;
        if (UnitManager.Instance == null || !UnitManager.Instance.IsAvailable(user)) return false;
        if (target != null && !UnitManager.Instance.IsAvailable(target)) return false;
        if (EffectProcessor.Instance == null) return false;
        if (item.RequiresSingleTarget && target == null && user.Data.IsEnemy)
        {
            foreach (var candidate in UnitManager.Instance.GetAllUnits())
            {
                if (!item.CanSelectTarget(user, candidate)) continue;
                target = candidate;
                break;
            }
        }
        if (item.RequiresSingleTarget && !item.CanSelectTarget(user, target)) return false;

        if (item.Effects == null || item.Effects.Count == 0)
        {
            return false;
        }

        EffectExecutionContext context = new EffectExecutionContext(user, target);
        EffectProcessor.Instance.EnqueueEffects(context, item.Effects);

        return true;
    }
}
