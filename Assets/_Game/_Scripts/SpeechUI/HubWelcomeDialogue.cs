using UnityEngine;


public class HubWelcomeDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSO _welcomeDialogue;
    [SerializeField] private SpeechUIController _speechUI;

    public void ShowDialogue()
    {
        if (_welcomeDialogue == null)
            Debug.LogError($"[HubWelcomeDialogue] Welcome Dialogue SO is not assigned on {gameObject.name}.");

        if (_speechUI == null)
            Debug.LogError($"[HubWelcomeDialogue] SpeechUIController is not assigned on {gameObject.name}.");

        AudioManager audioManager = FindAnyObjectByType<AudioManager>();
        PooledAudioSource source = null;

        if (audioManager != null && _welcomeDialogue.DialogueSound != null)
        {
            source = audioManager.PlayDialogue(_welcomeDialogue.DialogueSound, transform.position);
        }

        _speechUI.ShowDialogue(_welcomeDialogue, source, 0f);
    }

}
