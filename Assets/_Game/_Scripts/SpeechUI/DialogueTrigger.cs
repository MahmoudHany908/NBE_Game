using UnityEngine;


[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSO _dialogue;
    [SerializeField] private SpeechUIController _speechUI;

    [Header("Settings")]
    [Tooltip("If true, this trigger fires only once and then disables itself.")]
    [SerializeField] private bool _oneShot = true;

    private bool _hasFired;

    private void Awake()
    {
        if (_dialogue == null)
            Debug.LogError($"[DialogueTrigger] Dialogue SO is not assigned on {gameObject.name}.");

        if (_speechUI == null)
            Debug.LogError($"[DialogueTrigger] SpeechUIController is not assigned on {gameObject.name}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasFired && _oneShot) return;
        if (!other.CompareTag("Player")) return;

        if (_dialogue == null || _speechUI == null)
        {
            Debug.LogWarning($"[DialogueTrigger] Missing references on {gameObject.name}. Cannot show dialogue.");
            return;
        }

        _speechUI.ShowDialogue(_dialogue);
        Destroy(gameObject,0.3f);
         if (_oneShot)
             _hasFired = true;
    }
}
