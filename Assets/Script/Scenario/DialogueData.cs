using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueAction { Keep, Set }

[Serializable]
public class DialogueImage
{
    public DialogueAction action;
    public Sprite sprite;
}

[Serializable]
public class DialogueBGM
{
    public DialogueAction action;
    public AudioClip clip;
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(3, 8)] public string text;
    public DialogueImage left = new();
    public DialogueImage front = new();
    public DialogueImage right = new();   
    public DialogueImage background = new();
    public DialogueBGM bgm = new();
    [Min(0f)] public float blackoutDuration = 0f;
    [Min(0f)] public float nextLineDelay = 0f;
}

[CreateAssetMenu(menuName = "Game/Data/Dialogue")]
public class DialogueData : ScriptableObject
{    
    public List<DialogueLine> lines = new();
}
