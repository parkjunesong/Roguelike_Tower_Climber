using System;
using System.Collections.Generic;
using UnityEngine;
using CaveParallaxDemo;

public enum ScenarioStepType { Dialogue, Battle, Explore }

[Serializable]
public class ScenarioStep
{
    public ScenarioStepType type;
    public DialogueData dialogue;
    public BattleData battle;
    public MapData mapData;
}

[CreateAssetMenu(menuName = "Game/Data/Scenario")]
public class ScenarioData : ScriptableObject
{
    public string displayName;
    public string dialogueSceneName = "Talk";
    public string battleSceneName = "Battle";
    public string exploreSceneName = "explore";
    public string returnSceneName = "Main";
    public List<ScenarioStep> steps = new();
}
