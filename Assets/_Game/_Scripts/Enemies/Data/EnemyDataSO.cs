using UnityEngine;
using Alchemy.Inspector;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identity")] 
    public string enemyName = "Enemy";

    [Header("Health")] 
    public int maxHealth = 100;

    [Header("Movement")] 
    public float moveSpeed = 5f;

    [Header("Combat")] 
    public int attackDamage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("Behaviors")]
    [SerializeReference] public IMovementStrategy movementStrategy;
    [SerializeReference] public IAttackStrategy attackStrategy;

    [Header("Loot")]
    [Tooltip("Loot table defining XP and resource drops on kill. Leave null for no drops.")]
    public EnemyLootTableSO lootTable;
}
