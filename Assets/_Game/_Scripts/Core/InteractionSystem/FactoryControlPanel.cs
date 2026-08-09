using UnityEngine;
using VContainer;

public class FactoryControlPanel : MonoBehaviour, IInteractable
{
    public string InteractionText => "Open Factory Terminal";
    
    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
        Debug.Log($"[FactoryControlPanel] Constructed. EventPublisher is null? {_eventPublisher == null}");
    }

    public void Interact(Interactor interactor)
    {
        Debug.Log("[FactoryControlPanel] Interact triggered! Publishing Command_OpenFactoryUI.");
        _eventPublisher?.Publish(new Command_OpenFactoryUI());
    }

    private bool _requiresExit = false;

    public void RequireExit()
    {
        _requiresExit = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_requiresExit) return;

            Debug.Log("[FactoryControlPanel] Player walked into trigger! Auto-opening UI.");
            _eventPublisher?.Publish(new Command_OpenFactoryUI());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _requiresExit = false;
            Debug.Log("[FactoryControlPanel] Player left trigger! Auto-closing UI.");
            _eventPublisher?.Publish(new Command_CloseFactoryUI());
        }
    }
    
    public void CancelInteraction(Interactor interactor)
    {
        _eventPublisher?.Publish(new Command_CloseFactoryUI());
    }
}

public class Command_OpenFactoryUI : IGameEvent { }
public class Command_CloseFactoryUI : IGameEvent { }
