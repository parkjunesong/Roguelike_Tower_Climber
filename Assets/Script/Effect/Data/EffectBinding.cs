using System;
using UnityEngine;

public enum EffectTriggerType
{
    OnUse,          // ������ ���� ��� �� (�⺻��)
    OnTurnStart,    // �� ���� ��
    OnTurnEnd,      // �� ���� ��
    OnAttacked,     // �ǰ� �� (�ݰ�, ��ȣ�� ��)
    OnKill,         // �� óġ ��
    OnDeath         // ��� ��
}
public enum EffectTarget
{
    Self = 0,
    SingleEnemy = 1,
    SingleAlly = 5,
    AllAllies = 2,
    AllEnemies = 3,
    AllUnits = 4
}

[Serializable]
public class EffectBinding
{
    [Header("1. Basic Core")]
    [SerializeField] private EffectDefinition effectDefinition;
    [SerializeField] private EffectTarget target = EffectTarget.SingleEnemy;
    [SerializeField] private EffectTriggerType triggerType = EffectTriggerType.OnUse;

    [Header("2. Values")]
    [SerializeField, Min(0)] private int value = 1;
    [SerializeField, Min(0f)] private float multiplier = 1.0f;

    [Header("3. Conditions")]
    [SerializeField] private EffectCondition condition;

    [Header("4. Trigger Limits")]
    [SerializeField, Min(0)] private int maxTriggersPerTurn = 0; // 0�̸� ������  

    public EffectDefinition EffectDefinition => effectDefinition;
    public EffectTarget Target => target;
    public EffectTriggerType TriggerType => triggerType;
    public int MaxTriggersPerTurn => maxTriggersPerTurn;

    public int Value => value;
    public float Multiplier => multiplier;
    public int FinalValue => Mathf.RoundToInt(value * multiplier);
    public EffectCondition Condition => condition;       
}