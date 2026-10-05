using System.Collections.Generic;
using CaveParallaxDemo;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExplorationEntry
{
    public const string FormationSceneName = "PartyFormation";
    public static MapData PendingMap { get; private set; }
    private static string exploreScene;
    private static string returnScene;
    private static PlayerParty preparedParty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        PendingMap = null;
        exploreScene = null;
        returnScene = null;
        preparedParty = null;
    }

    public static bool Begin(MapData map, string destination = "explore", string cancelScene = "Main")
    {
        if (map == null || !map.Validate(out _) ||
            !Application.CanStreamedLevelBeLoaded(destination) ||
            !Application.CanStreamedLevelBeLoaded(cancelScene) ||
            !Application.CanStreamedLevelBeLoaded(FormationSceneName))
        {
            Debug.LogError("Exploration requires a valid map and formation, explore and return scenes.");
            return false;
        }
        PendingMap = map;
        exploreScene = destination;
        returnScene = cancelScene;
        preparedParty = null;
        SceneManager.LoadScene(FormationSceneName);
        return true;
    }

    public static bool Confirm(IReadOnlyList<Unit> members)
    {
        if (PendingMap == null || preparedParty != null ||
            SceneManager.GetActiveScene().name != FormationSceneName) return false;
        preparedParty = PlayerParty.Create(members);
        if (preparedParty == null) return false;
        SceneManager.LoadScene(exploreScene);
        return true;
    }

    public static bool TryConsume(out MapData map, out PlayerParty party)
    {
        map = PendingMap;
        party = preparedParty;
        if (map == null || party == null || !party.IsInitialized ||
            SceneManager.GetActiveScene().name != exploreScene) return false;
        Reset();
        return true;
    }

    public static void Cancel()
    {
        string destination = returnScene ?? "Main";
        Reset();
        if (ScenarioFlow.Current != null) ScenarioFlow.Cancel();
        else SceneManager.LoadScene(destination);
    }
}
