using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public int UnitId { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxHP => Status != null ? Status.GetFinalStat(UnitStatType.HP) : 0;
    public float HPPercent => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    public UnitData Data;
    public UnitStatus Status;
    //public UnitStatusController StatusController;
    public EffectTriggerListener TriggerListener;

    void OnDisable()
    {
        UnitManager.Instance.Unregister(this);
    }

    public virtual void Init(UnitData data)
    {
        Data = data;
        Status = gameObject.AddComponent<UnitStatus>();
        //StatusController = gameObject.AddComponent<UnitStatusController>();
        TriggerListener = gameObject.AddComponent<EffectTriggerListener>();      

        Status.Init(data.baseStats);
        TriggerListener.Init(this);

        CurrentHP = MaxHP;
    }

    public virtual void OnTurnStart()
    {
        // StatusController?.OnTurnStart();
        EventManager.TriggerTurnEvent(unit: this, EffectTriggerType.OnTurnStart);
    }
    public virtual void OnTurnEnd()
    {
        EventManager.TriggerTurnEvent(unit: this, EffectTriggerType.OnTurnEnd);
    }
    public virtual void OnDeath()
    {
        EventManager.TriggerCombatEvent(source: null, target: this, EffectTriggerType.OnDeath);
    }

    public virtual void OnDamaged(int value)
    {
        int bonusDamage = 0;
        int finalDamage = value + bonusDamage;
        CurrentHP = Mathf.Max(0, CurrentHP - finalDamage);

        EventManager.TriggerCombatEvent(source: null, target: this, EffectTriggerType.OnAttacked);

        Debug.Log($"���� ü�� ����! �⺻:{value}, �߰�:{bonusDamage}, ����:{finalDamage}, ���� HP:{CurrentHP}");

        if (CurrentHP <= 0)
        {
            OnDeath();
        }
    }

    public virtual void OnHealed(int value)
    {
        CurrentHP = Mathf.Min(CurrentHP + value, Status.GetFinalStat(UnitStatType.HP));
    }

    public virtual List<EffectBinding> GetPassiveEffects()
    {
        List<EffectBinding> passives = new List<EffectBinding>();
        /*
        if (Data == null || Data.Items == null) return passives;

        foreach (var item in Data.Items)
        {
            if (item == null || item.Effects == null) continue;

            foreach (var binding in item.Effects)
            {
                if (binding == null) continue;

                // OnUse�� �ƴ� ��� Ʈ���Ÿ� �нú� ȿ���� ����
                if (binding.TriggerType != EffectTriggerType.OnUse)
                {
                    passives.Add(binding);
                }
            }
        }
        */
        return passives;
    }

    /*
    public virtual bool HasBuff(BuffType buffType)
    {
        if (StatusController != null)
        {
            return StatusController.HasBuff(buffType);
        }
        return false;
    }
    */

    public void SetUnitId(int unitId)
    {
        UnitId = unitId;
    }  
}