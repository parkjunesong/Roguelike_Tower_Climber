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
            !Application.CanStreamedLevelBeLoaded(data.returnSceneName))
        {
            Debug.LogError("Assign a scenario with steps and a valid return scene.");
            return false;
        }
        foreach (var step in data.steps)
        {
            if (step == null ||
                (step.type == ScenarioStepType.Dialogue && (step.dialogue == null ||
                    !Application.CanStreamedLevelBeLoaded(data.dialogueSceneName))) ||
                (step.type == ScenarioStepType.Battle && (step.battle == null ||
                    !Application.CanStreamedLevelBeLoaded(data.battleSceneName))))
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
        SceneManager.LoadScene(scene);
    }

    private static void LoadStep()
    {
        if (CurrentStep == null)
        {
            Cancel();
            return;
        }
        SceneManager.LoadScene(CurrentStep.type == ScenarioStepType.Dialogue
            ? Current.dialogueSceneName : Current.battleSceneName);
    }
}
