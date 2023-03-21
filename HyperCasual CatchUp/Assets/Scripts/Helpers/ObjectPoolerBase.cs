using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// A basic class for simplifying an object pooling.
/// </summary>
/// <typeparam name="T">A MonoBehaviour object to perform pooling on.</typeparam>
public abstract class ObjectPoolerBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected T prefab;
    private ObjectPool<T> pool;

    protected ObjectPool<T> Pool
    {
        get
        {
            if (pool == null) throw new InvalidOperationException("You need to call InitPool before using it.");
            return pool;
        }
        set => pool = value;
    }

    protected void InitializePool(T prefab, int defaultCapacity = 10, int maxSize = 20, bool collectionChecks = false)
    {
        this.prefab = prefab;
        Pool = new ObjectPool<T>(
            CreatePooledObject,
            GetFromPool,
            ReleaseToPool,
            DestroyPooledObject,
            collectionChecks,
            defaultCapacity,
            maxSize);
    }

    #region OVERRIDES
    protected virtual T CreatePooledObject() => Instantiate(prefab);
    protected virtual void GetFromPool(T obj) => obj.gameObject.SetActive(true);
    protected virtual void ReleaseToPool(T obj) => obj.gameObject.SetActive(false);
    protected virtual void DestroyPooledObject(T obj) => Destroy(obj);
    #endregion

    #region GETTERS
    public T Get() => Pool.Get();
    public void Release(T obj) => Pool.Release(obj);
    #endregion
}