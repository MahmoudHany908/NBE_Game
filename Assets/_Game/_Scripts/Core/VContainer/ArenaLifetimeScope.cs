using VContainer;
using VContainer.Unity;

public class ArenaLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WaveSpawner>();
        builder.RegisterComponentInHierarchy<IronRevolver>();
    }
}
