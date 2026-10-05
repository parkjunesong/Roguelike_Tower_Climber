using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitStatType
{
    HP, // 체력
    AT, // 공격력
    DF, // 방어력
    CR, // 치명타 확률
    CD, // 치명타 데미지
    Count // 재행동 카운트, 비표시
}

[Serializable]
public class StatValue
{
    public UnitStatType type;
    public int value;
}

public class UnitStatus : MonoBehaviour
{    
    private Dictionary<UnitStatType, int> statDict;
    private UnitEquipment equipment;
   
    public void Init(List<StatValue> stats)
    {
        statDict = new();

        foreach (var stat in stats)
            statDict[stat.type] = stat.value;      
    }

    public int GetBaseStat(UnitStatType type)
    {
        return statDict.TryGetValue(type, out var value) ? value : 0;
    }

    // 버프 포함 최종 스탯 계산용
    public int GetFinalStat(UnitStatType type)
    {
        int value = GetBaseStat(type);
        if (equipment == null) equipment = GetComponent<UnitEquipment>();
        if (equipment != null) value += equipment.GetStatBonus(type);
        return type == UnitStatType.HP ? Mathf.Max(1, value) : value;
    }
}
