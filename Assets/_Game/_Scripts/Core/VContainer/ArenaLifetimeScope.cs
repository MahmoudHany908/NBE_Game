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
        builder.RegisterBuildCallback(container =>
        {
            var workshopManager = container.Resolve<ICombatWorkshopManager>();
            if (workshopManager != null)
            {
                var equipped = workshopManager.GetEquippedCharacters();
                foreach (var character in equipped)
                {
                    if (character != null && character.AbilityRef != null && character.AbilityRef.abilityPrefab != null)
                    {
                        var abilityInstance = Instantiate(character.AbilityRef.abilityPrefab);
                        container.InjectGameObject(abilityInstance);
                    }
                }
            }
        });
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