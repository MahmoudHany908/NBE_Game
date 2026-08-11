using UnityEngine;
using VContainer;

public class GateTrigger : MonoBehaviour
{
    // (No longer using targetSceneName since UI handles it)

    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        // Debug.Log("[GateTrigger] Construct called! Injecting EventPublisher.");
        _eventPublisher = eventPublisher;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"[GateTrigger] Trigger entered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            // Debug.Log($"[GateTrigger] Player entered gate. Opening Loadout UI...");

            if (_eventPublisher == null)
            {
                Debug.LogError("[GateTrigger] CRITICAL ERROR: _eventPublisher is NULL! VContainer injection failed.");
            }
            else
            {
                _eventPublisher.Publish(new Command_OpenGateLoadoutUI());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _eventPublisher?.Publish(new Command_CloseGateLoadoutUI());
        }
    }
}