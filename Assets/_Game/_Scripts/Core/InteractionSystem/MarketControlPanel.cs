using UnityEngine;
using VContainer;
using VaultSurvivors.Core.Market;

public class MarketControlPanel : MonoBehaviour, IInteractable
{
    public string InteractionText => "Open Market";
    
    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public void Interact(Interactor interactor)
    {
        _eventPublisher?.Publish(new Command_OpenMarketUI());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _eventPublisher?.Publish(new Command_OpenMarketUI());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _eventPublisher?.Publish(new Command_CloseMarketUI());
        }
    }
    
    public void CancelInteraction(Interactor interactor)
    {
        _eventPublisher?.Publish(new Command_CloseMarketUI());
    }
}
