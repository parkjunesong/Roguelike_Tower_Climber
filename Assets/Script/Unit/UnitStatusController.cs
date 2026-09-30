using System.Collections.Generic;
using UnityEngine;

/*
public enum BuffType
{
    None = 0,

    // 상태 이상 (Debuff)
    Burn,         // 화상
    Poison,       // 중독
    Stun,         // 빙결 / 기절
    Bleed,        // 출혈

    // 능력치 / 상태 버프 (Buff)
    AttackUp,     // 공격력 증가
    DefenseUp,    // 방어력 증가
    Shield,       // 보호막
    Taunt         // 도발
}

public class UnitStatusController : MonoBehaviour
{
    private Dictionary<BuffType, BuffInstance> activeBuffs = new();

    public bool HasBuff(BuffType buffType)
    {
        return activeBuffs.ContainsKey(buffType) && activeBuffs[buffType].RemainingTurns > 0;
    }

    public void AddBuff(BuffType buffType, BuffInstance buff)
    {
        if (activeBuffs.ContainsKey(buffType))
        {
            // 턴수 연장 또는 위력 중첩 처리
            activeBuffs[buffType].RemainingTurns += buff.RemainingTurns;
        }
        else
        {
            activeBuffs.Add(buffType, buff);
        }
    }

    public void OnTurnStart()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].RemainingTurns--;

            if (activeBuffs[i].RemainingTurns <= 0)
                activeBuffs.RemoveAt(i);
        }
    }
}
*/