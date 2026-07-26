using UnityEngine;
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
        builder.RegisterComponentInHierarchy<RookAbility>();
        builder.RegisterComponentInHierarchy<JessyAbility>();
        builder.RegisterComponentInHierarchy<DoctorAbility>();
        builder.RegisterComponentInHierarchy<SeeleAbility>();


    }

    private void Start()
    {
        if (Container != null)
        {
            var milestoneController = Container.Resolve<WaveMilestoneController>();
            if (milestoneController != null)
            {
                milestoneController.BeginRun();
            }
        }
    }
}