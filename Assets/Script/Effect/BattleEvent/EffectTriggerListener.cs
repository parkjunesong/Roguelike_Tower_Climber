using System.Collections.Generic;
using UnityEngine;

public class EffectTriggerListener : MonoBehaviour
{
    private Unit ownerUnit;
    private Dictionary<EffectBinding, int> triggerCounts = new();

    private void OnEnable()
    {
        EventManager.OnTurnEvent -= HandleTurnEvent;
        EventManager.OnTurnEvent += HandleTurnEvent;

        EventManager.OnCombatEvent -= HandleCombatEvent;
        EventManager.OnCombatEvent += HandleCombatEvent;
    }

    private void OnDisable()
    {
        EventManager.OnTurnEvent -= HandleTurnEvent;
        EventManager.OnCombatEvent -= HandleCombatEvent;
    }

    public void Init(Unit unit)
    {
        ownerUnit = unit;
    }

    // �� ���� �� ���� ������ ī��Ʈ ��ϸ� �ʱ�ȭ
    public void ResetTurnCounts()
    {
        triggerCounts.Clear();
    }

    public int GetTriggerCount(EffectBinding binding)
    {
        return triggerCounts.TryGetValue(binding, out int count) ? count : 0;
    }

    public void IncrementTriggerCount(EffectBinding binding)
    {
        if (triggerCounts.ContainsKey(binding))
            triggerCounts[binding]++;
        else
            triggerCounts[binding] = 1;
    }

    private void HandleTurnEvent(Unit unit, EffectTriggerType trigger)
    {
        if (unit != ownerUnit) return;

        // �� ���� ���۵� �� �ߵ� Ƚ�� ����
        if (trigger == EffectTriggerType.OnTurnStart)
        {
            ResetTurnCounts();
        }

        ProcessTrigger(trigger, null);
    }

    private void HandleCombatEvent(Unit source, Unit target, EffectTriggerType trigger)
    {
        if (source != ownerUnit && target != ownerUnit) return;

        Unit selectedTarget = (source == ownerUnit) ? target : source;
        ProcessTrigger(trigger, selectedTarget);
    }

    private void ProcessTrigger(EffectTriggerType trigger, Unit target)
    {
        if (ownerUnit == null) return;

        List<EffectBinding> passiveEffects = ownerUnit.GetPassiveEffects();
        if (passiveEffects == null || passiveEffects.Count == 0) return;

        EffectExecutionContext context = new EffectExecutionContext(ownerUnit, target, trigger);
        EffectProcessor.Instance.EnqueueEffects(context, passiveEffects);
    }
}