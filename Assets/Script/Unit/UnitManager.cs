using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    private List<Unit> Units = new();
    private int nextUnitId;
    private readonly Dictionary<Unit, int> allySlots = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        nextUnitId = 0;
    }

    public void Register(Unit unit)
    {
        if (unit == null || unit.IsDead || Units.Contains(unit))
            return;

        unit.SetUnitId(nextUnitId++);
        if (!unit.Data.IsEnemy)
        {
            int slot = 0;
            while (allySlots.ContainsValue(slot)) slot++;
            allySlots[unit] = slot;
        }
        Units.Add(unit);
        UpdateFormation();
    }

    public void Unregister(Unit unit)
    {
        if (unit != null && Units.Remove(unit))
        {
            allySlots.Remove(unit);
            UpdateFormation();
        }
    }

    private void UpdateFormation()
    {
        var allies = Units.Where(u => u != null && u.Data != null && !u.Data.IsEnemy).ToList();
        var enemies = Units.Where(u => u != null && u.Data != null && u.Data.IsEnemy).ToList();

        Vector2[] allyPositions =
        {
            new Vector2(-5f, -2.5f),
            new Vector2(-10f, -2.5f),
            new Vector2(-15f, -2.5f)
        };

        foreach (var ally in allies)
        {
            if (!allySlots.TryGetValue(ally, out int slot) || slot >= allyPositions.Length) continue;
            var position = allyPositions[slot];
            ally.transform.position = new Vector3(position.x, position.y, ally.transform.position.z);
        }

        Vector2[] enemyPositions = enemies.Count switch
        {
            1 => new[]
            {
                new Vector2(10f, -2.5f)
            },
            2 => new[]
            {
                new Vector2(7.5f, -2.5f), new Vector2(12.5f, -2.5f)
            },
            3 => new[]
            {
                new Vector2(5f, -2.5f), new Vector2(10f, -2.5f), new Vector2(15f, -2.5f)
            },
            4 => new[]
            {
                new Vector2(6.5f, -1.75f), new Vector2(7.5f, -3.25f),
                new Vector2(11.5f, -1.75f), new Vector2(12.5f, -3.25f)
            },
            5 => new[]
            {
                new Vector2(5f, -3.25f), new Vector2(7.5f, -1.75f), new Vector2(10f, -3.25f),
                new Vector2(12.5f, -1.75f), new Vector2(15f, -3.25f)
            },
            6 => new[]
            {
                new Vector2(4f, -1.75f), new Vector2(5f, -3.25f),
                new Vector2(9f, -1.75f), new Vector2(10f, -3.25f),
                new Vector2(14f, -1.75f), new Vector2(15f, -3.25f)
            },
            _ => null
        };

        if (enemyPositions == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            var position = enemyPositions[i];
            enemies[i].transform.position = new Vector3(position.x, position.y, enemies[i].transform.position.z);
        }
    }
    public void OnTurnStart()
    {
        foreach (var unit in GetAllUnits())
            unit.OnTurnStart();
    }

    public void OnTurnEnd()
    {
        foreach (var unit in GetAllUnits())
            unit.OnTurnEnd();
    }

    public Unit GetUnit(int unitId)
    {
        return Units.FirstOrDefault(unit => IsAvailable(unit) && unit.UnitId == unitId);
    }

    public bool IsAvailable(Unit unit)
    {
        return unit != null && !unit.IsDead && unit.isActiveAndEnabled && unit.CurrentHP > 0 && Units.Contains(unit);
    }

    public IReadOnlyList<Unit> GetAlliesOf(Unit source)
    {
        return source == null ? new List<Unit>() :
            Units.Where(u => IsAvailable(u) && u.Data.IsEnemy == source.Data.IsEnemy).ToList();
    }

    public IReadOnlyList<Unit> GetEnemiesOf(Unit source)
    {
        return source == null ? new List<Unit>() :
            Units.Where(u => IsAvailable(u) && u.Data.IsEnemy != source.Data.IsEnemy).ToList();
    }

    public IReadOnlyList<Unit> GetAllUnits() => Units.Where(IsAvailable).ToList();
}
