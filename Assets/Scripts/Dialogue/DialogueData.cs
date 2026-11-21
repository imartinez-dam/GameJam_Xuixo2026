using System;
using UnityEngine;

public enum Speaker
{
    Presenter,
    Chef
}

[Serializable]
public struct DialogueLine
{
    public Speaker speaker;
    [TextArea(2, 4)] public string text;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Xuixo/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}