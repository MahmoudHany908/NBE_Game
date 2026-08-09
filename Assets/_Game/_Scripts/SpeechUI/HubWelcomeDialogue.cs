using UnityEngine;


public class HubWelcomeDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSO _welcomeDialogue;
    [SerializeField] private SpeechUIController _speechUI;

    [Header("Settings")]
    [SerializeField] private float _delayBeforeShow = 3f;

    private void Start()
    {
        if (_welcomeDialogue == null)
            Debug.LogError($"[HubWelcomeDialogue] Welcome Dialogue SO is not assigned on {gameObject.name}.");

        if (_speechUI == null)
            Debug.LogError($"[HubWelcomeDialogue] SpeechUIController is not assigned on {gameObject.name}.");

        _speechUI.ShowDialogue(_welcomeDialogue, _delayBeforeShow);
    }
    

}
