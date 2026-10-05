using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public int currentWave;
    private IReadOnlyList<WaveData> waves;
    private IReadOnlyList<UnitData> initialPlayers;
    private bool battleStarted;
    private bool advanceScenario;
    private bool usePersistentParty;
    public event System.Action<bool> BattleFinished;
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
        if (!hasAllies) Finish(false);
        else if (!hasEnemies) NextWave();
    }

    public void Init(BattleData data, bool advanceScenario = true, bool usePersistentParty = false)
    {
        Configure(data?.Waves, data?.Players, advanceScenario, usePersistentParty);
    }

    public void InitEncounter(UnitData enemy)
    {
        Configure(new List<WaveData> { new WaveData { UnitData = new List<UnitData> { enemy } } }, null, false, true);
    }

    private void Configure(IReadOnlyList<WaveData> enemyWaves, IReadOnlyList<UnitData> players, bool advanceScenario, bool usePersistentParty)
    {
        waves = enemyWaves;
        initialPlayers = players;
        this.advanceScenario = advanceScenario;
        this.usePersistentParty = usePersistentParty;
        currentWave = 0;
        battleStarted = false;
        IsFinished = false;
        IsVictory = false;
    }

    public void BattleStart()
    {
        if (battleStarted || IsFinished) return;
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("Assign enemy waves to start a battle.", this);
            return;
        }
        foreach (var wave in waves)
        {
            if (wave == null || wave.UnitData == null || wave.UnitData.Count == 0)
            {
                Debug.LogError("Battle waves require enemy UnitData.", this);
                return;
            }
            foreach (var enemy in wave.UnitData)
                if (enemy == null || !enemy.IsEnemy)
                {
                    Debug.LogError("Battle waves must contain enemy UnitData.", this);
                    return;
                }
        }
        if (usePersistentParty)
        {
            if (PlayerParty.Instance == null || !PlayerParty.Instance.IsInitialized)
            {
                Debug.LogError("Initialize the exploration party before starting a battle.", this);
                return;
            }
        }
        else
        {
            int playerCount = 0;
            if (initialPlayers != null)
                foreach (var player in initialPlayers)
                {
                    if (player == null) continue;
                    if (player.IsEnemy)
                    {
                        Debug.LogError("Battle player slots must contain player UnitData.", this);
                        return;
                    }
                    playerCount++;
                }
            if (playerCount == 0)
            {
                Debug.LogError("Assign at least one player to start a battle.", this);
                return;
            }
        }
        TurnManager.Instance.Init();
        if (usePersistentParty)
            PlayerParty.Instance.Deploy(UnitSpawner.Instance, UnitManager.Instance);
        else
            foreach (var player in initialPlayers)
                if (player != null) UnitSpawner.Instance.Spawn(player);
        battleStarted = true;
        bool hasAlivePlayer = false;
        foreach (var unit in UnitManager.Instance.GetAllUnits())
            if (!unit.Data.IsEnemy) hasAlivePlayer = true;
        if (!hasAlivePlayer) { Finish(false); return; }
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
        if (currentWave >= waves.Count) { Finish(true); return; }
        foreach (var enemy in waves[currentWave].UnitData) UnitSpawner.Instance.Spawn(enemy);
        Debug.Log($"Wave {currentWave + 1} Start");
    }

    private void Finish(bool victory)
    {
        if (IsFinished) return;
        Debug.Log(victory ? "Battle Clear" : "Battle Defeat");
        battleStarted = false;
        IsFinished = true;
        IsVictory = victory;
        if (usePersistentParty && PlayerParty.Instance != null) PlayerParty.Instance.Suspend();
        foreach (var unit in UnitSpawner.Instance.GetComponentsInChildren<Unit>(true))
        {
            if (unit.Data == null || !unit.Data.IsEnemy) continue;
            unit.gameObject.SetActive(false);
            Destroy(unit.gameObject);
        }
        BattleFinished?.Invoke(victory);
        if (!advanceScenario) return;
        if (victory) ScenarioFlow.CompleteStep(ScenarioStepType.Battle);
        else ScenarioFlow.Cancel();
    }
}
