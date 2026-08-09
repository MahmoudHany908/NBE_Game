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

        _speechUI.ShowDialogue(_dialogue);

        AudioManager audioManager = FindAnyObjectByType<AudioManager>();
        Player player = other.GetComponent<Player>();
        float waitTime = 0f;

        if (audioManager != null && _dialogue.DialogueSound != null)
        {
            audioManager.PlaySoundAtPosition(_dialogue.DialogueSound, transform.position);
            
            if (_dialogue.DialogueSound.clips != null && _dialogue.DialogueSound.clips.Length > 0)
            {
                waitTime = _dialogue.DialogueSound.clips[0].length;
            }
        }

        if (player != null)
        {
            StartCoroutine(HandlePlayerWait(player, waitTime));
        }
        else
        {
            Destroy(gameObject, waitTime + 0.1f);
        }
    }

    private System.Collections.IEnumerator HandlePlayerWait(Player player, float waitTime)
    {
        PlayerInputs inputs = player.GetPlayerInputs();
        Rigidbody rb = player.GetPlayerRigidBody();

        if (inputs != null)
        {
            inputs.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        if (inputs != null)
        {
            inputs.enabled = true;
        }

        Destroy(gameObject);
    }
}
