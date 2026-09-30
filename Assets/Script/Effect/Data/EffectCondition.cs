using System;
using UnityEngine;

public enum ConditionType
{
    Always,              // �׻� �ߵ�
    HealthBelowPercent,  // ü�� N% ����
    //HasBuff,             // ��󿡰� Ư�� ����/������� �ɷ����� ��
    TargetCount,         // ��� ���� ���� N�� �̻�/������ ��
    RandomChance         // Ȯ�� �ߵ�
}

[Serializable]
public class EffectCondition
{
    [SerializeField] private ConditionType type = ConditionType.Always;
    [SerializeField] private float value;
    //[SerializeField] private BuffType requiredBuff = BuffType.None; // EffectTag ��� BuffType ���

    public ConditionType Type => type;
    public float Value => value;
    //public BuffType RequiredBuff => requiredBuff;
}