using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    public static EnemyRegistry Instance { get; private set; }
 
    private readonly List<Enemy> activeEnemies = new List<Enemy>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(Enemy enemy)
    {
        if(!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void Unregister(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public IReadOnlyList<Enemy> GetActiveEnemies()
    {
        return activeEnemies;
    }
}