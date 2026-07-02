using UnityEngine;

public class SecurityDrone : Enemy
{
    protected override void Awake()
    {
        moveSpeed = 6f;
        base.Awake();
        GetComponent<HealthComponent>().SetMaxHealth(40);
    }
}