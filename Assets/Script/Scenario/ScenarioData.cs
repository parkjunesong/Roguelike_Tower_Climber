using System;
using System.Collections.Generic;
using UnityEngine;

public enum ScenarioStepType { Dialogue, Battle }

[Serializable]
public class ScenarioStep
{
    public ScenarioStepType type;
    public DialogueData dialogue;
    public BattleData battle;
}

[CreateAssetMenu(menuName = "Game/Data/Scenario")]
public class ScenarioData : ScriptableObject
{
    public string displayName;
    public string dialogueSceneName = "talk";
    public string battleSceneName = "Battle";
    public string returnSceneName = "main";
    public List<ScenarioStep> steps = new();
}
