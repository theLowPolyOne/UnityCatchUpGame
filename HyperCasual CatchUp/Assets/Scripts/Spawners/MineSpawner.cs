using Fallencake.Entities.Characters;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using Fallencake.Damage;

public class MineSpawner : ObjectPoolerBase<Mine>
{
    #region FIELDS

    [SerializeField] private Mine minePrefab;
    [SerializeField] private Transform container;
    [SerializeField] private int maxSpawned = 100;
    [SerializeField] private float spawnTimestep = 1f;

    private List<Mine> mines = new List<Mine>();
    private NavMeshTriangulation Triangulation;
    private bool isSpawning;

    public int MaxSpawned { get => maxSpawned; set => maxSpawned = value; }
    public float SpawnTimestep { get => spawnTimestep; set => spawnTimestep = value; }

    #endregion

    #region MONO AND INITIALIZATION

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Triangulation = NavMesh.CalculateTriangulation();
        InitializePool(minePrefab, maxSize: maxSpawned); // Initialize the pool
        //for (int i = 0; i < maxSpawned; i++)
        //{
        //    var enemy = Get(); // Get an enemy instance from the pool
        //    mines.Add(enemy);
        //}
        //foreach (var enemy in mines)
        //{
        //    Release(enemy); // Release the enemy back to the pool
        //}
    }

    #endregion

    protected override Mine CreatePooledObject()
    {
        var mine = Instantiate(prefab, container);
        mine.SetPool(Pool);
        mine.gameObject.name += $" {Pool.CountAll}";
        mine.gameObject.SetActive(false);
        return mine;
    }

    protected override void GetFromPool(Mine mine)
    {
        base.GetFromPool(mine);
        int VertexIndex = Random.Range(0, Triangulation.vertices.Length);
        NavMeshHit Hit;
        if (NavMesh.SamplePosition(Triangulation.vertices[VertexIndex], out Hit, 2f, -1))
        {
            mine.Warp(Hit.position);
        }
        else
        {
            Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {Triangulation.vertices[VertexIndex]}");
        }
    }

    protected override void ReleaseToPool(Mine mine)
    {
        base.ReleaseToPool(mine);
        if (!isSpawning && Pool.CountInactive > 0) StartCoroutine(SpawnMine());
    }

    private IEnumerator SpawnMine()
    {
        isSpawning = true;
        while (Pool.CountActive < maxSpawned)
        {
            yield return new WaitForSeconds(spawnTimestep); //Wait for the spawn interval before spawning the next object
            Get();
        }
        isSpawning = false;
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnMine());
    }

    public void ResetToDefault()
    {
        foreach (var mine in mines)
        {
            Release(mine);
        }
        StopAllCoroutines();
    }
}
