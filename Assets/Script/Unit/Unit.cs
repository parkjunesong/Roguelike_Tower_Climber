using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    private UnitEquipment equipment;
    public UnitEquipment Equipment
    {
        get
        {
            if (equipment == null)
                equipment = GetComponent<UnitEquipment>() ?? gameObject.AddComponent<UnitEquipment>();
            return equipment;
        }
    }

    public int UnitId { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }
    private Unit lastDamageSource;
    public int MaxHP => Status != null ? Status.GetFinalStat(UnitStatType.HP) : 0;
    public float HPPercent => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    public UnitData Data;
    public UnitStatus Status;
    //public UnitStatusController StatusController;
    public EffectTriggerListener TriggerListener;

    void OnDisable()
    {
        if (UnitManager.Instance != null)
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
        IsDead = false;
        lastDamageSource = null;
    }

    public virtual void OnTurnStart()
    {
        // StatusController?.OnTurnStart();
        if (IsDead) return;
        EventManager.TriggerTurnEvent(unit: this, EffectTriggerType.OnTurnStart);
    }
    public virtual void OnTurnEnd()
    {
        if (IsDead) return;
        EventManager.TriggerTurnEvent(unit: this, EffectTriggerType.OnTurnEnd);
    }
    public virtual void OnDeath()
    {
        if (IsDead) return;
        IsDead = true;
        CurrentHP = 0;

        EventManager.TriggerCombatEvent(source: lastDamageSource, target: this, EffectTriggerType.OnDeath);
        if (lastDamageSource != null && lastDamageSource != this)
            EventManager.TriggerCombatEvent(source: lastDamageSource, target: this, EffectTriggerType.OnKill);

        if (TriggerListener != null) TriggerListener.enabled = false;
        foreach (var animator in GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (var renderer in GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
        foreach (var canvas in GetComponentsInChildren<Canvas>(true)) canvas.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
        if (UnitManager.Instance != null) UnitManager.Instance.Unregister(this);
        StartCoroutine(RemoveAfterEffects());
    }

    private IEnumerator RemoveAfterEffects()
    {
        // Keep the source available to queued OnDeath effects until the queue drains.
        yield return null;
        while (EffectProcessor.Instance != null && EffectProcessor.Instance.IsProcessing)
            yield return null;
        Destroy(gameObject);
    }

    public virtual void OnDamaged(int value, Unit source = null)
    {
        if (IsDead || value <= 0) return;
        lastDamageSource = source;
        int bonusDamage = 0;
        int finalDamage = value + bonusDamage;
        CurrentHP = Mathf.Max(0, CurrentHP - finalDamage);

        EventManager.TriggerCombatEvent(source: source, target: this, EffectTriggerType.OnAttacked);

        Debug.Log($"HP:{CurrentHP}");

        if (CurrentHP <= 0)
        {
            OnDeath();
        }
    }

    public virtual void OnHealed(int value)
    {
        if (IsDead || value <= 0) return;
        CurrentHP = Mathf.Min(CurrentHP + value, MaxHP);
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
