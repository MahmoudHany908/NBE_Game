using UnityEngine;

public interface IInteractable
{
    string InteractionText { get; }
    public void Interact(Interactor interactor);
    public void CancelInteraction(Interactor interactor);

}
