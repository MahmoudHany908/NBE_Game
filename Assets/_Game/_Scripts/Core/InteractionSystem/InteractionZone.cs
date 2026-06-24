using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionZone : MonoBehaviour
{
    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponentInParent<IInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactor interactor))
        {
            interactor.SetInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactor interactor))
        {
            interactor.ClearInteractable(interactable);
        }
    }
}