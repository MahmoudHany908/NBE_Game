using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Button _interactButtonPrefab;
    [SerializeField] private GameObject parentUI;

    private IInteractable _currentInteractable;
    private bool _isInteracting;

    private Button _interactButton;

    private void OnDisable()
    {
        if (_interactButton != null)
            _interactButton.onClick.RemoveListener(() => OnInteractButtonClicked(null));
    }

    private void OnInteractButtonClicked(IInteractable interactable)
    {
        if (interactable == null) return;
        interactable.Interact(this);

    }

    public void SetInteractable(IInteractable interactable)
    {
        _currentInteractable = interactable;
        if (_isInteracting) return;

        if (_interactButton == null)
        {
            _interactButton = GameObject.Instantiate(_interactButtonPrefab, parentUI.transform);
            TextMeshProUGUI txt = _interactButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = interactable.InteractionText;
        }
        _isInteracting = true;
        _interactButton.onClick.AddListener(() => OnInteractButtonClicked(interactable));
    }
    public void ClearInteractable(IInteractable interactable)
    {
        interactable.CancelInteraction(this);
        if (_currentInteractable != null)
            _interactButton.onClick.RemoveListener(() => OnInteractButtonClicked(interactable));
        Destroy(_interactButton.gameObject);

        _isInteracting = false;
        _currentInteractable = null;
    }


}