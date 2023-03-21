using UnityEngine.Pool;
using UnityEngine.AI;
using UnityEngine;
using System.Collections;

namespace Fallencake.Damage
{
    public class Mine : MonoBehaviour
    {
        [Header("COMPONENTS:")]
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Transform visualbody;
        [SerializeField] private Explosion explosion;
        [SerializeField] private float releseAfterDeathTime = 2f;

        private IObjectPool<Mine> _pool;
        public void SetPool(IObjectPool<Mine> pool) => _pool = pool;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered mine trap!");
                Explode();
            }
            else
            {
                Debug.Log("Not Player entered mine trap!");
            }
        }

        public void Warp(Vector3 position)
        {
            navMeshAgent.Warp(position);
        }

        private void Explode()
        {
            explosion.transform.parent = null;
            explosion.transform.position = transform.position;
            explosion.Explode();
            StartCoroutine(ResetToDefault());
        }

        private IEnumerator ResetToDefault()
        {
            yield return new WaitForSeconds(releseAfterDeathTime);
            explosion.transform.parent = transform;
            explosion.transform.position = transform.position;
            _pool.Release(this);
        }
    }
}