using Fallencake.CharacterController;
using System.Collections;
using Fallencake.Damage;
using UnityEngine.Pool;
using UnityEngine;

namespace Fallencake.Entities.Characters
{
    [DisallowMultipleComponent]
    public class Enemy : MonoBehaviour
    {
        public Health Health;
        public EnemyController Movement;

        [SerializeField] private Explosion explosion;
        [SerializeField] private float releseAfterDeathTime = 2f;

        private IObjectPool<Enemy> _pool;
        public void SetPool(IObjectPool<Enemy> pool) => _pool = pool;


        private void Start()
        {
            Health.OnDeath += Die;
            Movement.OnAttack += Explode;
        }

        private void Die()
        {
            Movement.StopMoving();
            StartCoroutine(HandleDeath());
        }

        private void Explode()
        {
            explosion.transform.parent = null;
            explosion.transform.position = transform.position;
            explosion.Explode();
        }

        public IEnumerator HandleSpawn()
        {
            //Spawn tween
            yield return new WaitForSeconds(releseAfterDeathTime);
        }

        private IEnumerator HandleDeath()
        {
            yield return new WaitForSeconds(releseAfterDeathTime);
            ResetTodefault();
            _pool.Release(this);
        }

        public void ResetTodefault()
        {
            explosion.transform.parent = transform;
            explosion.transform.position = transform.position;
            Movement.ResetToDefault();
        }
    }
}