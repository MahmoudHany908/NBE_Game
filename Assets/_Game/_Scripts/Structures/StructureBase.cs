using UnityEngine;
using TMPro;

public class StructureBase : MonoBehaviour, IInteractable
{
    [SerializeField] private string _name;
    [SerializeField] private PanelUI _structureUI;
    [SerializeField] private TextMeshProUGUI _structureNameText;

    public string InteractionText => _name;



    public void Interact(Interactor interactor)
    {
        _structureUI.Show();
        _structureNameText.text = _name;
    }

    public void CancelInteraction(Interactor interactor)
    {
        _structureUI.Hide();
    }
}
