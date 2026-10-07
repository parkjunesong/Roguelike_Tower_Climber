using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ScenarioBattleButton : MonoBehaviour
{
    [SerializeField] private ScenarioData scenarioData;
    [SerializeField] private BattleData battleData;
    [SerializeField] private string battleSceneName = "Battle";
    private bool loading;

    public static BattleData SelectedBattleData { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSelection()
    {
        SelectedBattleData = null;
    }

    public static BattleData ConsumeSelection()
    {
        BattleData data = SelectedBattleData;
        SelectedBattleData = null;
        return data;
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(StartBattle);
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(StartBattle);
    }

    public void StartBattle()
    {
        if (loading || ScenarioFlow.Current != null || ExplorationEntry.PendingMap != null)
            return;

        if (scenarioData != null)
        {
            ScenarioFlow.Begin(scenarioData);
            return;
        }

        if (battleData == null || !Application.CanStreamedLevelBeLoaded(battleSceneName))
        {
            Debug.LogError("Assign BattleData and a battle scene included in the build settings.", this);
            return;
        }

        loading = true;
        SelectedBattleData = battleData;
        SceneManager.LoadScene(battleSceneName);
    }
}
