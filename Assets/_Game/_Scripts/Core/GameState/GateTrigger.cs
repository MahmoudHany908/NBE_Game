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
            Debug.Log($"[GateTrigger] Player entered gate. Opening Loadout UI...");

            if (_eventPublisher == null)
            {
                Debug.LogError("[GateTrigger] CRITICAL ERROR: _eventPublisher is NULL! VContainer injection failed.");
            }
            else
            {
                _eventPublisher.Publish(new Command_OpenGateLoadoutUI());
            }
            VaultSurvivors.LoadingSystem.SceneLoader.Load(targetSceneName, new ArenaState());
            Debug.Log("[GateTrigger] SceneLoader requested transition.");
        }
    }
}