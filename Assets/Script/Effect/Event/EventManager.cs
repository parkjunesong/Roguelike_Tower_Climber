using System;

public static class EventManager
{
    public static event Action<Unit, EffectTriggerType> OnTurnEvent;
    public static event Action<Unit, Unit, EffectTriggerType> OnCombatEvent;

    // �� ���� �̺�Ʈ �߻� �˸� (OnTurnStart, OnTurnEnd ��)
    public static void TriggerTurnEvent(Unit unit, EffectTriggerType trigger)
    {
        OnTurnEvent?.Invoke(unit, trigger);
    }

    // ����/���� ��ȣ�ۿ� �̺�Ʈ �߻� �˸� (OnAttacked, OnKill, OnDeath ��)
    public static void TriggerCombatEvent(Unit source, Unit target, EffectTriggerType trigger)
    {
        OnCombatEvent?.Invoke(source, target, trigger);
    }
}