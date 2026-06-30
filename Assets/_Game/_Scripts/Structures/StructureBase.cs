using UnityEngine;
using TMPro;

public class StructureBase : MonoBehaviour, IInteractable
{
    [Header("Structure Data")]
    [SerializeField] private StructureSO structureData;

    [Header("References")]
    [SerializeField] private PanelUI structurePanelUI;
    [SerializeField] private TextMeshProUGUI structureNameText;

    [Header("other")]
    public string InteractionText => structureData.InteractionText;

    private IStructureBehaviour structureBehaviour;

    void Awake()
    {
        structureBehaviour = GetComponent<IStructureBehaviour>();
    }

    public void Interact(Interactor interactor)
    {
        structurePanelUI.Show();
        structureNameText.text = structureData.Name;

        if (structureBehaviour is FactoryBehavior factory)
        {
            factory.CollectResources();
        }
    }

    public void CancelInteraction(Interactor interactor)
    {
        structurePanelUI.Hide();
    }

    // Helper accessor script so the UI can easily query values from this facility object instance
    public IStructureBehaviour GetBehaviour() => structureBehaviour;
}