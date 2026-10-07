using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{
    public const string SceneName = "Main";
    public static MainSceneController Instance { get; private set; }
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private GameObject formationRoot;
    [SerializeField] private TalkSceneController dialogue;
    [SerializeField] private PartyFormationController formation;
    public bool IsFormationOpen => formationRoot.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance() => Instance = null;
    private void Awake() => Instance = this;
    private void Start() => Refresh();
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public static void ShowMain()
    {
        if (SceneManager.GetActiveScene().name == SceneName && Instance != null) Instance.Refresh();
        else SceneManager.LoadScene(SceneName);
    }

    private void Refresh()
    {
        menuRoot.SetActive(false);
        dialogueRoot.SetActive(false);
        formationRoot.SetActive(false);
        var step = ScenarioFlow.CurrentStep;
        if (step != null && step.type == ScenarioStepType.Dialogue)
        {
            dialogueRoot.SetActive(true);
            dialogue.BeginDialogue(step.dialogue);
        }
        else if (ExplorationEntry.PendingMap != null)
        {
            formationRoot.SetActive(true);
            formation.Open();
        }
        else menuRoot.SetActive(true);
    }
}
