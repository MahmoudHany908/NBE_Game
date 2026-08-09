using UnityEngine;

/// <summary>
/// ScriptableObject that holds a single dialogue message.
/// Create via: Right-click > Create > NBE/Dialogue
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "NBE/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [TextArea(3, 8)]
    [SerializeField] private string _dialogueText;

    [SerializeField] private SoundData _dialogueSound;

    /// <summary>The text to display in the Speech UI panel.</summary>
    public string DialogueText => _dialogueText;

    /// <summary>The sound to play when the dialogue is shown.</summary>
    public SoundData DialogueSound => _dialogueSound;
}
