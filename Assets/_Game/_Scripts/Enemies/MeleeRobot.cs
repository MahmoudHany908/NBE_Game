using UnityEngine;

public class MeleeRobot : Enemy
{
    protected override void Awake()
    {
        moveSpeed = 5f;
        base.Awake();
        GetComponent<HealthComponent>().SetMaxHealth(80f);
    }
}