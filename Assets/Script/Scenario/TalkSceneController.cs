using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TalkSceneController : MonoBehaviour
{
    [SerializeField] private DialogueData previewDialogue;
    [SerializeField] private Text nameField;
    [SerializeField] private Text talkField;
    [SerializeField] private Image background;
    [SerializeField] private Image left;
    [SerializeField] private Image front;
    [SerializeField] private Image right;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button menuButton;
    public UnityEvent<DialogueLine> ChoiceRequested = new();
    public UnityEvent<string> ChoiceSelected = new();

    private DialogueData dialogue;
    private int lineIndex = -1;
    private Coroutine presentation;
    private bool typing;
    private bool auto;
    private bool waitingForChoice;
    private bool finished;

    private void Start()
    {
        advanceButton.onClick.AddListener(Advance);
        if (autoButton != null) autoButton.onClick.AddListener(ToggleAuto);
        if (menuButton != null) menuButton.onClick.AddListener(ReturnToMain);
        var step = ScenarioFlow.CurrentStep;
        dialogue = step != null && step.type == ScenarioStepType.Dialogue
            ? step.dialogue : previewDialogue;
        if (dialogue == null)
        {
            Debug.LogError("Assign a preview DialogueData or enter from a scenario.", this);
            return;
        }
        Advance();
    }

    public void Advance()
    {
        if (dialogue == null || finished || waitingForChoice) return;
        if (presentation != null) StopCoroutine(presentation);
        if (typing)
        {
            typing = false;
            talkField.text = dialogue.lines[lineIndex].text;
            presentation = StartCoroutine(AfterLine(dialogue.lines[lineIndex]));
            return;
        }
        lineIndex++;
        if (lineIndex >= dialogue.lines.Count)
        {
            finished = true;
            ScenarioFlow.CompleteStep(ScenarioStepType.Dialogue);
            return;
        }
        var line = dialogue.lines[lineIndex];
        nameField.text = line.speaker;
        ApplyImage(background, line.background);
        ApplyImage(left, line.left);
        ApplyImage(front, line.front);
        ApplyImage(right, line.right);
        if (line.soundEffect != null && soundSource != null)
            soundSource.PlayOneShot(line.soundEffect);
        presentation = StartCoroutine(Present(line));
    }

    private IEnumerator Present(DialogueLine line)
    {
        string text = line.text ?? "";
        talkField.text = "";
        typing = true;
        if (line.charactersPerSecond > 0)
        {
            for (int i = 1; i <= text.Length; i++)
            {
                talkField.text = text.Substring(0, i);
                yield return new WaitForSecondsRealtime(1f / line.charactersPerSecond);
            }
        }
        talkField.text = text;
        typing = false;
        yield return AfterLine(line);
    }

    private IEnumerator AfterLine(DialogueLine line)
    {
        if (line.choices != null && line.choices.Count > 0)
        {
            waitingForChoice = true;
            ChoiceRequested.Invoke(line);
            yield break;
        }
        float elapsed = 0f;
        while (!finished)
        {
            if (auto)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= line.autoDelay)
                {
                    Advance();
                    yield break;
                }
            }
            else elapsed = 0f;
            yield return null;
        }
    }

    // Future choice UI can call this method with the selected option index.
    public void SelectChoice(int index)
    {
        if (!waitingForChoice) return;
        var choices = dialogue.lines[lineIndex].choices;
        if (index < 0 || index >= choices.Count) return;
        waitingForChoice = false;
        ChoiceSelected.Invoke(choices[index].eventId);
        Advance();
    }

    public void ToggleAuto() => auto = !auto;

    public void ReturnToMain()
    {
        if (ScenarioFlow.Current != null) ScenarioFlow.Cancel();
    }

    private static void ApplyImage(Image image, DialogueImage data)
    {
        if (image == null || data == null || data.action == DialogueImageAction.Keep) return;
        image.sprite = data.action == DialogueImageAction.Set ? data.sprite : null;
        image.enabled = image.sprite != null;
    }

    private void OnDestroy()
    {
        if (advanceButton != null) advanceButton.onClick.RemoveListener(Advance);
        if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAuto);
        if (menuButton != null) menuButton.onClick.RemoveListener(ReturnToMain);
    }
}
