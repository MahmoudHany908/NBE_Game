using UnityEngine;
using VContainer;

public class GateTrigger : MonoBehaviour
{
    // Make sure this is set to "OsamaCombat" in the Unity Inspector!
    [SerializeField] private string targetSceneName = "ArenaScene";

    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        Debug.Log("[GateTrigger] Construct called! Injecting EventPublisher.");
        _eventPublisher = eventPublisher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[GateTrigger] Player entered gate. Requesting transition to {targetSceneName}...");

            if (_eventPublisher == null)
            {
                Debug.LogError("[GateTrigger] CRITICAL ERROR: _eventPublisher is NULL! VContainer injection failed.");
            }
            else
            {
                Debug.Log("[GateTrigger] _eventPublisher is valid. Publishing ChangeStateCommand...");
                _eventPublisher.Publish(new ChangeStateCommand
                {
                    TargetSceneName = targetSceneName,
                    TargetState = new ArenaState()
                });
                Debug.Log("[GateTrigger] ChangeStateCommand published.");
            }
        }
    }
}