
public static class SkillService
{
    public static bool UseSkill(SkillDefinition item, Unit user, Unit target = null)
    {
        if (item == null || user == null) return false;

        if (item.Effects == null || item.Effects.Count == 0)
        {
            return false;
        }

        EffectExecutionContext context = new EffectExecutionContext(user, target);
        EffectProcessor.Instance.EnqueueEffects(context, item.Effects);

        return true;
    }
}