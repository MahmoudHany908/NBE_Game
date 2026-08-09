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

        if (_oneShot)
            _hasFired = true;

        AudioManager audioManager = FindAnyObjectByType<AudioManager>();
        PooledAudioSource source = null;

        if (audioManager != null && _dialogue.DialogueSound != null)
        {
            source = audioManager.PlayDialogue(_dialogue.DialogueSound, transform.position);
        }

        _speechUI.ShowDialogue(_dialogue, source);

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            StartCoroutine(HandlePlayerWait(player));
        }
        else
        {
            StartCoroutine(HandleSelfDestroy());
        }
    }

    private System.Collections.IEnumerator HandlePlayerWait(Player player)
    {
        PlayerInputs inputs = player.GetPlayerInputs();
        Rigidbody rb = player.GetPlayerRigidBody();

        if (inputs != null) inputs.enabled = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        while (_speechUI.IsShowing)
        {
            yield return null;
        }

        if (inputs != null) inputs.enabled = true;
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator HandleSelfDestroy()
    {
        while (_speechUI.IsShowing)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
