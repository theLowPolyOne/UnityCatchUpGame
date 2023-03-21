using Fallencake.Entities.Characters;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;

public class EnemySpawner : ObjectPoolerBase<Enemy>
{
    #region FIELDS

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private int maxSpawned = 20;
    [SerializeField] private float spawnTimestep = 1f;

    public int MaxSpawned { get => maxSpawned; set => maxSpawned = value; }
    public float SpawnTimestep { get => spawnTimestep; set => spawnTimestep = value; }

    private bool isSpawning;
    private List<Enemy> enemies = new List<Enemy>();

    #endregion

    #region MONO AND INITIALIZATION

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        InitializePool(enemyPrefab, maxSize: maxSpawned); // Initialize the pool
        //for (int i = 0; i < maxSpawned; i++)
        //{
        //    var enemy = Get(); // Get an enemy instance from the pool
        //    enemies.Add(enemy);
        //}
        //foreach (var enemy in enemies)
        //{
        //    Release(enemy); // Release the enemy back to the pool
        //}
    }

    #endregion

    protected override Enemy CreatePooledObject()
    {
        var enemy = Instantiate(prefab, enemyContainer);
        enemy.SetPool(Pool);
        enemy.gameObject.name += $" {Pool.CountAll}";
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    protected override void GetFromPool(Enemy enemy)
    {
        base.GetFromPool(enemy);
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
        enemy.transform.position = spawnPosition;
        StartCoroutine(enemy.HandleSpawn());
    }

    protected override void ReleaseToPool(Enemy enemy)
    {
        base.ReleaseToPool(enemy);
        if (!isSpawning && Pool.CountInactive > 0) StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;
        while (Pool.CountActive < maxSpawned)
        {
            // Wait for the spawn interval before spawning the next object
            yield return new WaitForSeconds(spawnTimestep);
            // Spawn the enemy and increment the spawn count
            Get();
        }
        isSpawning = false;
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemies());
    }
}