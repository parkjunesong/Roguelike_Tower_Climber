using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalkSceneController : MonoBehaviour
{
    private const float CharactersPerSecond = 30f;
    private const float AutoDelay = 1.5f;

    [SerializeField] private DialogueData previewDialogue;
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text talkField;
    [SerializeField] private Image background;
    [SerializeField] private Image left;
    [SerializeField] private Image front;
    [SerializeField] private Image right;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Button autoButton;

    private DialogueData dialogue;
    private int lineIndex = -1;
    private Coroutine presentation;
    private bool typing;
    private bool auto;
    private bool finished;

    private void Start()
    {
        advanceButton.onClick.AddListener(Advance);
        if (autoButton != null) autoButton.onClick.AddListener(ToggleAuto);
        var step = ScenarioFlow.CurrentStep;
        dialogue = step != null && step.type == ScenarioStepType.Dialogue
            ? step.dialogue : previewDialogue;
        if (dialogue == null)
        {
            Debug.LogError("Assign a preview DialogueData or enter from a scenario.", this);
            return;
        }
        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
        bgmSource.Stop();
        bgmSource.clip = null;
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        Advance();
    }

    public void Advance()
    {
        if (dialogue == null || finished) return;
        if (presentation != null) StopCoroutine(presentation);
        presentation = null;
        if (typing)
        {
            typing = false;
            talkField.text = dialogue.lines[lineIndex].text;
            presentation = StartCoroutine(AfterLine());
            return;
        }
        lineIndex++;
        if (lineIndex >= dialogue.lines.Count)
        {
            finished = true;
            if (bgmSource != null) bgmSource.Stop();
            ScenarioFlow.CompleteStep(ScenarioStepType.Dialogue);
            return;
        }
        var line = dialogue.lines[lineIndex];
        nameField.text = line.speaker;
        ApplyImage(background, line.background);
        ApplyImage(left, line.left);
        ApplyImage(front, line.front);
        ApplyImage(right, line.right);
        ApplyBgm(line.bgm);
        presentation = StartCoroutine(Present(line));
    }

    private IEnumerator Present(DialogueLine line)
    {
        string text = line.text ?? "";
        talkField.text = "";
        typing = true;
        for (int i = 1; i <= text.Length; i++)
        {
            talkField.text = text.Substring(0, i);
            yield return new WaitForSecondsRealtime(1f / CharactersPerSecond);
        }
        talkField.text = text;
        typing = false;
        yield return AfterLine();
    }

    private IEnumerator AfterLine()
    {
        float elapsed = 0f;
        while (!finished)
        {
            if (auto)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= AutoDelay)
                {
                    presentation = null;
                    Advance();
                    yield break;
                }
            }
            else elapsed = 0f;
            yield return null;
        }
    }

    public void ToggleAuto() => auto = !auto;

    public void ReturnToMain()
    {
        if (ScenarioFlow.Current != null) ScenarioFlow.Cancel();
    }

    private static void ApplyImage(Image image, DialogueImage data)
    {
        if (image == null || data == null || data.action == DialogueAction.Keep) return;
        image.sprite = data.action == DialogueAction.Set ? data.sprite : null;
        image.enabled = image.sprite != null;
    }

    private void ApplyBgm(DialogueBGM data)
    {
        if (bgmSource == null || data == null || data.action == DialogueAction.Keep) return;
        if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = data.clip;
        if (bgmSource.clip != null) bgmSource.Play();
    }

    private void OnDestroy()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (advanceButton != null) advanceButton.onClick.RemoveListener(Advance);
        if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAuto);
    }
}
