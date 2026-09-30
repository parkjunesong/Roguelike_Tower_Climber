using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    private List<Unit> Units = new();
    private int nextUnitId;

    [Header("Formation")]
    [SerializeField, Min(0f)] private float groupSpacing = 2f;
    [SerializeField, Min(0f)] private float unitSpacing = 0.75f;

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
        if (unit == null || Units.Contains(unit))
            return;

        unit.SetUnitId(nextUnitId++);
        Units.Add(unit);
        UpdateFormation();
    }

    public void Unregister(Unit unit)
    {
        if (unit != null && Units.Remove(unit))
            UpdateFormation();
    }

    private void UpdateFormation()
    {
        ArrangeSide(Units.Where(u => u != null && !u.Data.IsEnemy).ToList(), -1f);
        ArrangeSide(Units.Where(u => u != null && u.Data.IsEnemy).ToList(), 1f);
    }

    private void ArrangeSide(List<Unit> units, float direction)
    {
        float edge = transform.position.x + direction * groupSpacing * 0.5f;
        foreach (var unit in units)
        {
            var renderer = unit.GetComponent<SpriteRenderer>();
            float width = renderer != null && renderer.sprite != null ? renderer.bounds.size.x : 1f;
            float centerOffset = renderer != null ? renderer.bounds.center.x - unit.transform.position.x : 0f;
            unit.transform.position = new Vector3(edge + direction * width * 0.5f - centerOffset,
                transform.position.y, transform.position.z);
            edge += direction * (width + unitSpacing);
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
        return unit != null && unit.isActiveAndEnabled && unit.CurrentHP > 0 && Units.Contains(unit);
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