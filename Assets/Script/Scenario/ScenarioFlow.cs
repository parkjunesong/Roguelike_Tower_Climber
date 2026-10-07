using UnityEngine;
using UnityEngine.SceneManagement;

public static class ScenarioFlow
{
    public static ScenarioData Current { get; private set; }
    public static int StepIndex { get; private set; }
    public static ScenarioStep CurrentStep => Current != null && StepIndex < Current.steps.Count
        ? Current.steps[StepIndex] : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Current = null;
        StepIndex = 0;
    }

    public static bool Begin(ScenarioData data)
    {
        if (data == null || data.steps == null || data.steps.Count == 0 ||
            !Application.CanStreamedLevelBeLoaded(data.returnSceneName) ||
            !Application.CanStreamedLevelBeLoaded(MainSceneController.SceneName))
        {
            Debug.LogError("Assign a scenario with steps and a valid return scene.");
            return false;
        }
        foreach (var step in data.steps)
        {
            if (step == null ||
                (step.type == ScenarioStepType.Dialogue && step.dialogue == null) ||
                (step.type == ScenarioStepType.Battle && (step.battle == null ||
                    !Application.CanStreamedLevelBeLoaded(data.battleSceneName))) ||
                (step.type == ScenarioStepType.Explore && (step.mapData == null || !step.mapData.Validate(out _) ||
                    !Application.CanStreamedLevelBeLoaded(data.exploreSceneName))))
            {
                Debug.LogError("Scenario steps require data and scenes included in build settings.", data);
                return false;
            }
        }
        Current = data;
        StepIndex = 0;
        LoadStep();
        return true;
    }

    public static void CompleteStep(ScenarioStepType type)
    {
        if (CurrentStep == null || CurrentStep.type != type) return;
        StepIndex++;
        LoadStep();
    }

    public static void Cancel()
    {
        if (Current == null) return;
        string scene = Current.returnSceneName;
        Reset();
        ExplorationEntry.Reset();
        if (scene == MainSceneController.SceneName) MainSceneController.ShowMain();
        else SceneManager.LoadScene(scene);
    }

    private static void LoadStep()
    {
        if (CurrentStep == null)
        {
            Cancel();
            return;
        }
        if (CurrentStep.type == ScenarioStepType.Explore)
        {
            ExplorationEntry.Begin(CurrentStep.mapData, Current.exploreSceneName, Current.returnSceneName);
            return;
        }
        if (CurrentStep.type == ScenarioStepType.Dialogue)
        {
            MainSceneController.ShowMain();
            return;
        }
        SceneManager.LoadScene(Current.battleSceneName);
    }
}
