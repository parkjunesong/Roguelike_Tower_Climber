using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public int currentWave;

    private BattleData battleData;

    public void Init(BattleData data)
    {
        battleData = data;
        currentWave = 0;
    }

    public void BattleStart()
    {
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
        SpawnCurrentWave();
        TurnManager.Instance.AdvanceTurn();
    }

    public void NextWave()
    {
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
        Debug.Log("Battle Clear");
    }
}