using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int CurrentTurn { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init()
    {
        CurrentTurn = 0;
    }

    public void AdvanceTurn()
    {
        CurrentTurn++;

        Debug.Log($"Turn {CurrentTurn}");

        UnitManager.Instance.OnTurnStart();
    }
}