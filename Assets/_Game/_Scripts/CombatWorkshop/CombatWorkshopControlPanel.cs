using UnityEngine;
using VContainer;

public class CombatWorkshopControlPanel : MonoBehaviour, IInteractable
{
    public string InteractionText => "Open Combat Workshop";
    
    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public void Interact(Interactor interactor)
    {
        _eventPublisher?.Publish(new Command_OpenCombatWorkshopUI());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _eventPublisher?.Publish(new Command_OpenCombatWorkshopUI());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _eventPublisher?.Publish(new Command_CloseCombatWorkshopUI());
        }
    }
    
    public void CancelInteraction(Interactor interactor)
    {
        _eventPublisher?.Publish(new Command_CloseCombatWorkshopUI());
    }
}
