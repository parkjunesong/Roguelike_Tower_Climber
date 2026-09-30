using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    private List<Unit> Units = new();
    private int nextUnitId;

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
        if (unit == null)
            return;

        if (Units.Contains(unit))
            return;

        unit.SetUnitId(nextUnitId++);
        Units.Add(unit);

        UpdateFormation();
    }

    public void Unregister(Unit unit)
    {
        if (unit == null)
            return;

        if (Units.Remove(unit))
        {
            UpdateFormation();
        }
    }

    private void UpdateFormation()
    {
        List<Unit> enemyUnits = Units.OfType<Enemy>().Cast<Unit>().ToList();

        int count = enemyUnits.Count;
        if (count == 0) return;

        float spacing = 5f;
        float startX = -(count - 1) * spacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 position = Vector3.zero;
            position.x = startX + i * spacing;

            enemyUnits[i].transform.position = position;
        }
    }

    public void OnTurnStart()
    {
        foreach (var unit in Units.ToArray())
        {
            unit.OnTurnStart();
        }
    }

    public void OnTurnEnd()
    {
        foreach (var unit in Units.ToArray())
        {
            unit.OnTurnEnd();
        }
    }

    public Unit GetUnit(int unitId)
    {
        return Units.FirstOrDefault(unit => unit.UnitId == unitId);
    }
    public IReadOnlyList<Unit> GetAlliesOf(Unit source)
    {
        // source가 Enemy면 모든 Enemy가 아군, 플레이어면 Enemy가 아닌 모든 유닛이 아군
        return Units.Where(u => (u is Enemy) == source is Enemy).ToList();
    }
    public IReadOnlyList<Unit> GetEnemiesOf(Unit source)
    {
        // source가 Enemy면 Enemy가 아닌 유닛(플레이어)이 적, 플레이어면 모든 Enemy가 적
        return Units.Where(u => (u is Enemy) != source is Enemy).ToList();
    }
    public IReadOnlyList<Unit> GetAllUnits() => Units;
}