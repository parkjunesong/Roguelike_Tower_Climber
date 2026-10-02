using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueImageAction { Keep, Set, Clear }

[Serializable]
public class DialogueImage
{
    public DialogueImageAction action;
    public Sprite sprite;
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public string eventId;
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(3, 8)] public string text;
    public DialogueImage background = new();
    public DialogueImage left = new();
    public DialogueImage front = new();
    public DialogueImage right = new();
    public AudioClip soundEffect;
    [Min(0f)] public float charactersPerSecond = 30f;
    [Min(0f)] public float autoDelay = 1.5f;
    [Tooltip("Reserved for future choice UI. Lines with choices pause and raise ChoiceRequested.")]
    public List<DialogueChoice> choices = new();
}

[CreateAssetMenu(menuName = "Game/Data/Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new();
}
