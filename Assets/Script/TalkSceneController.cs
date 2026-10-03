using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalkSceneController : MonoBehaviour
{
    private const float CharactersPerSecond = 30f;
    private const float AutoDelay = 1.5f;
    private const float BackgroundFadeDuration = 0.4f;

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
    private bool transitioning;
    private Image backgroundBlend;
    private Image blackout;

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
        if (dialogue == null || finished || transitioning) return;
        if (presentation != null) StopCoroutine(presentation);
        presentation = null;
        if (typing)
        {
            typing = false;
            talkField.text = dialogue.lines[lineIndex].text;
            presentation = StartCoroutine(AfterLine());
            return;
        }
        presentation = StartCoroutine(ShowNextLine());
    }

    private IEnumerator ShowNextLine()
    {
        transitioning = true;
        bool hasNextLine = lineIndex >= 0 && lineIndex + 1 < dialogue.lines.Count;
        var previousLine = hasNextLine ? dialogue.lines[lineIndex] : null;
        if (previousLine != null && previousLine.nextLineDelay > 0f)
            yield return new WaitForSecondsRealtime(previousLine.nextLineDelay);

        float blackoutDuration = previousLine != null ? Mathf.Max(0f, previousLine.blackoutDuration) : 0f;
        bool useBlackout = blackoutDuration > 0f;
        float blackoutStart = 0f;
        if (useBlackout)
        {
            if (blackout == null)
            {
                var canvas = talkField.canvas.rootCanvas;
                blackout = CreateOverlay("Dialogue Blackout", canvas.transform);
                blackout.color = new Color(0f, 0f, 0f, 0f);
                blackout.raycastTarget = true;
            }
            blackout.transform.SetAsLastSibling();
            blackout.enabled = true;
            blackoutStart = Time.realtimeSinceStartup;
            yield return FadeImage(blackout, 0f, 1f, blackoutDuration / 3f);
        }

        lineIndex++;
        if (lineIndex >= dialogue.lines.Count)
        {
            finished = true;
            transitioning = false;
            if (bgmSource != null) bgmSource.Stop();
            ScenarioFlow.CompleteStep(ScenarioStepType.Dialogue);
            yield break;
        }
        var line = dialogue.lines[lineIndex];
        nameField.text = line.speaker;
        talkField.text = "";
        ApplyImage(left, line.left);
        ApplyImage(front, line.front);
        ApplyImage(right, line.right);
        ApplyBgm(line.bgm);
        if (useBlackout)
        {
            // Reserve the middle third for background blending and a fully black hold.
            float revealStart = blackoutStart + blackoutDuration * 2f / 3f;
            float blendDuration = Mathf.Min(BackgroundFadeDuration,
                Mathf.Max(0f, revealStart - Time.realtimeSinceStartup));
            yield return TransitionBackground(line.background, blendDuration);
            float holdDuration = revealStart - Time.realtimeSinceStartup;
            if (holdDuration > 0f) yield return new WaitForSecondsRealtime(holdDuration);
            float revealDuration = Mathf.Max(0f,
                blackoutStart + blackoutDuration - Time.realtimeSinceStartup);
            yield return FadeImage(blackout, 1f, 0f, revealDuration);
            blackout.enabled = false;
        }
        else yield return TransitionBackground(line.background);
        transitioning = false;
        yield return Present(line);
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

    private IEnumerator TransitionBackground(DialogueImage data, float duration = BackgroundFadeDuration)
    {
        if (background == null || data == null || data.action == DialogueAction.Keep) yield break;
        if (background.sprite == data.sprite && background.enabled == (data.sprite != null)) yield break;

        Color originalColor = background.color;
        if (data.sprite == null)
        {
            if (background.enabled)
                yield return FadeImage(background, originalColor.a, 0f, duration);
        }
        else
        {
            if (backgroundBlend == null)
            {
                backgroundBlend = CreateOverlay("Dialogue Background Blend", background.transform);
                // Keep the blend underneath character images parented to the background.
                backgroundBlend.transform.SetAsFirstSibling();
                backgroundBlend.type = background.type;
                backgroundBlend.preserveAspect = background.preserveAspect;
                backgroundBlend.material = background.material;
            }
            if (background.sprite == null) background.enabled = false;
            backgroundBlend.sprite = data.sprite;
            backgroundBlend.color = originalColor;
            backgroundBlend.enabled = true;
            yield return FadeImage(backgroundBlend, 0f, originalColor.a, duration);
        }

        ApplyImage(background, data);
        background.color = originalColor;
        if (backgroundBlend != null) backgroundBlend.enabled = false;
    }

    private static Image CreateOverlay(string name, Transform parent)
    {
        var overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(parent, false);
        var image = overlay.GetComponent<Image>();
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private static IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        Color color = image.color;
        color.a = from;
        image.color = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            image.color = color;
        }
        color.a = to;
        image.color = color;
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
        if (backgroundBlend != null) Destroy(backgroundBlend.gameObject);
        if (blackout != null) Destroy(blackout.gameObject);
        if (bgmSource != null) bgmSource.Stop();
        if (advanceButton != null) advanceButton.onClick.RemoveListener(Advance);
        if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAuto);
    }
}
