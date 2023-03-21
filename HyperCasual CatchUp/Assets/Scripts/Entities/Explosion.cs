using Fallencake.CharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fallencake.Damage
{
    public class Explosion : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private ParticleSystem particleSystemPrefab;
        [SerializeField] private int maxHits = 25;
        [SerializeField] private float radius = 10f;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private LayerMask blockExplosionLayers;
        [SerializeField] private int maxDamage = 50;
        [SerializeField] private int minDamage = 10;
        [SerializeField] private float explosivePower;
        private Collider[] targets;

        #endregion

        private void Awake()
        {
            targets = new Collider[maxHits];
        }

        public void Explode()
        {
            particleSystemPrefab.Play();
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, targets, hitLayers);
            for (int i = 0; i < hits; i++)
            {
                if (!Physics.Raycast(transform.position, (targets[i].transform.position - transform.position).normalized, radius, blockExplosionLayers.value))
                {
                    float distance = Vector3.Distance(transform.position, targets[i].transform.position);
                    float damage = Mathf.Lerp(maxDamage, minDamage, distance / radius);
                    if (targets[i].TryGetComponent<IDamageable>(out IDamageable damageable))
                    {                        
                        damageable.TakeDamage(Mathf.FloorToInt(damage));
                        //Debug.Log($"Hit {targets[i].name} for {damage}");
                    }
                    else if (targets[i].TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
                    {
                        rigidbody.AddForce(Vector3.up * explosivePower, ForceMode.Impulse);
                        rigidbody.AddExplosionForce(explosivePower, transform.position, radius);
                        //Debug.Log($"Would hit {rigidbody.name} for {damage}");
                    }
                }
            }
        }
    }
}