using System.Collections.Generic;
using CaveParallaxDemo;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExplorationEntry
{
    public static MapData PendingMap { get; private set; }
    private static string exploreScene;
    private static string returnScene;
    private static PlayerParty preparedParty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    internal static void Reset()
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
            !Application.CanStreamedLevelBeLoaded(MainSceneController.SceneName))
        {
            Debug.LogError("Exploration requires a valid map and Main, explore and return scenes.");
            return false;
        }
        PendingMap = map;
        exploreScene = destination;
        returnScene = cancelScene;
        preparedParty = null;
        MainSceneController.ShowMain();
        return true;
    }

    public static bool Confirm(IReadOnlyList<Unit> members)
    {
        if (PendingMap == null || preparedParty != null ||
            SceneManager.GetActiveScene().name != MainSceneController.SceneName ||
            MainSceneController.Instance == null || !MainSceneController.Instance.IsFormationOpen) return false;
        if (members == null) return false;
        foreach (var unit in members)
            if (unit == null || unit.Equipment.GetItem(0)?.Definition.WeaponClass is null or UnitClass.None) return false;
        preparedParty = PlayerParty.Create(members);
        if (preparedParty == null) return false;
        foreach (var unit in preparedParty.Units) unit.Equipment.TryLockExplorationClass(out _);
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
        else if (destination == MainSceneController.SceneName) MainSceneController.ShowMain();
        else SceneManager.LoadScene(destination);
    }
}
