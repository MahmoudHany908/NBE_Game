using VContainer;
using VContainer.Unity;

public class ArenaLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WaveSpawner>();
        builder.RegisterComponentInHierarchy<IronRevolver>();
        builder.RegisterComponentInHierarchy<DeathUIController>();
        builder.RegisterComponentInHierarchy<ExtractionUIController>();
        builder.RegisterComponentInHierarchy<WaveMilestoneController>();
        builder.RegisterComponentInHierarchy<PlayerHealthComponent>();

    }

    private void Start()
    {
        var milestoneController = GetComponentInChildren<WaveMilestoneController>();
        if (milestoneController != null)
        {
            milestoneController.BeginRun();
        }
    }
}