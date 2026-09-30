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
        UnitSpawner.Instance.Spawn(battleData.Player);
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