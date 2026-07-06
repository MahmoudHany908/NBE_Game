using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AudioManager audioManager;


    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(poolManager);
        builder.RegisterComponent(audioManager);
        poolManager.gameObject.transform.parent = null;
        audioManager.gameObject.transform.parent = null;
    }
}