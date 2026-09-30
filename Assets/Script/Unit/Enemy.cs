using UnityEngine;

public class Enemy : Unit
{
    [SerializeField] private UnitHPBar HPBar;
    [SerializeField] private UnitActionCounter ACText;

    private int actionCounter;

    public override void Init(UnitData data)
    {
        base.Init(data);

        actionCounter = Status.GetFinalStat(UnitStatType.Count) + 1;

        //HPBar.Init(CurrentHP, CurrentHP);
        //ACText.SetCounter(actionCounter);
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();

        actionCounter--;

        if (actionCounter < 0)
        {
            Act();

            actionCounter = Status.GetFinalStat(UnitStatType.Count);
        }

        //ACText.SetCounter(actionCounter);
    }

    public override void OnDamaged(int value)
    {
        base.OnDamaged(value);
        HPBar.SetHP(CurrentHP);
    }

    public override void OnHealed(int value)
    {
        base.OnHealed(value);
        HPBar.SetHP(CurrentHP);
    }

    private void Act()
    {
        //ItemService.UseItem(Data.items[0], this);
    }
}