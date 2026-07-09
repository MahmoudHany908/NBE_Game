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
        _eventPublisher = eventPublisher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered gate. Requesting transition to {targetSceneName}...");

            _eventPublisher?.Publish(new ChangeStateCommand
            {
                TargetSceneName = targetSceneName,
                TargetState = new ArenaState()
            });
        }
    }
}