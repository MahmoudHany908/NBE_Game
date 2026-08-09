using UnityEngine;

[System.Serializable]
public struct DialoguePart
{
    [TextArea(3, 8)]
    public string text;
    [Tooltip("The timestamp in seconds in the audio clip where this part starts.")]
    public float startTime;
}

/// <summary>
/// ScriptableObject that holds a dialogue message with parts.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "NBE/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] private SoundData _dialogueSound;
    [SerializeField] private DialoguePart[] _dialogueParts;

    public SoundData DialogueSound => _dialogueSound;
    public DialoguePart[] DialogueParts => _dialogueParts;
}
