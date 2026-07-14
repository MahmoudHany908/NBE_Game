using UnityEngine;
using VContainer;
using VaultSurvivors.Core.Production;

public class FactoryBehavior : MonoBehaviour, IStructureBehaviour
{
    [Header("Factory Configuration")]
    public string factoryId = "Main_Energy_Factory_01";

    private IProductionManager _productionManager;

    [Inject]
    public void Construct(IProductionManager productionManager)
    {
        _productionManager = productionManager;
    }

    private void Start()
    {
        // Mostly a visual/physical shell, since IProductionManager handles the core logic.
    }

    public void Tick(float deltaTime)
    {
        // The VContainer ITickable interface in ProductionManager handles its own updates,
        // but this fulfills the IStructureBehaviour contract.
        // If you choose to remove ITickable from ProductionManager and drive it from here instead:
        // _productionManager?.Tick(); 
    }

    public void Upgrade()
    {
        if (_productionManager != null)
        {
            _productionManager.TryUpgrade();
        }
    }
}