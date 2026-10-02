using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public int currentWave;

    private BattleData battleData;
    private bool battleStarted;
    public bool IsFinished { get; private set; }
    public bool IsVictory { get; private set; }

    private void Update()
    {
        if (!battleStarted || IsFinished || UnitManager.Instance == null ||
            (EffectProcessor.Instance != null && EffectProcessor.Instance.IsProcessing)) return;

        bool hasAllies = false;
        bool hasEnemies = false;
        foreach (var unit in UnitManager.Instance.GetAllUnits())
        {
            if (unit.Data.IsEnemy) hasEnemies = true;
            else hasAllies = true;
        }

        if (!hasAllies) BattleDefeat();
        else if (!hasEnemies) NextWave();
    }

    public void Init(BattleData data)
    {
        battleData = data;
        currentWave = 0;
        battleStarted = false;
        IsFinished = false;
        IsVictory = false;
    }

    public void BattleStart()
    {
        if (battleStarted || IsFinished) return;
        if (battleData == null || battleData.Players == null || battleData.Players.Count != 3)
        {
            Debug.LogError("Assign exactly three player UnitData entries to BattleData.Players.", this);
            return;
        }

        for (int i = 0; i < battleData.Players.Count; i++)
        {
            UnitData player = battleData.Players[i];
            if (player == null || player.IsEnemy)
            {
                Debug.LogError($"Assign a player UnitData to party slot {i + 1}.", this);
                return;
            }
        }

        foreach (UnitData player in battleData.Players)
        {
            UnitSpawner.Instance.Spawn(player);
        }
        battleStarted = true;
        SpawnCurrentWave();
        if (!IsFinished) TurnManager.Instance.AdvanceTurn();
    }

    public void NextWave()
    {
        if (!battleStarted || IsFinished) return;
        currentWave++;
        SpawnCurrentWave();
    }

    private void SpawnCurrentWave()
    {
        if (currentWave >= battleData.WaveCount)
        {
            BattleClear();
            return;
        }

        WaveData wave = battleData.Waves[currentWave];
        foreach (var unit in wave.UnitData)
        {
            UnitSpawner.Instance.Spawn(unit);
        }

        Debug.Log($"Wave {currentWave + 1} Start");
    }
    private void BattleClear()
    {
        if (IsFinished) return;
        Debug.Log("Battle Clear");
        battleStarted = false;
        IsFinished = true;
        IsVictory = true;
        ScenarioFlow.CompleteStep(ScenarioStepType.Battle);
    }

    private void BattleDefeat()
    {
        Debug.Log("Battle Defeat");
        battleStarted = false;
        IsFinished = true;
        IsVictory = false;
        ScenarioFlow.Cancel();
    }
}