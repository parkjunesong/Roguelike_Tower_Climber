using UnityEngine;

public class BattleSceneInitializer : MonoBehaviour
{
    public BattleData battleData;

    void Start()
    {
        var selectedBattleData = ScenarioBattleButton.ConsumeSelection();
        var scenarioStep = ScenarioFlow.CurrentStep;
        if (scenarioStep != null && scenarioStep.type == ScenarioStepType.Battle)
            selectedBattleData = scenarioStep.battle;
        if (selectedBattleData != null)
            battleData = selectedBattleData;

        if (battleData == null)
            return;

        var battleManager = GetComponent<BattleManager>();
        battleManager.Init(battleData);
        battleManager.BattleStart();       
    }  
}
