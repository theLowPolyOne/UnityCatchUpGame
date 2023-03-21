using UnityEngine;

namespace Fallencake.Damage
{
    [DisallowMultipleComponent]
    public class PainResponse : MonoBehaviour
    {
        [SerializeField] private ParticleSystem deathFX;
        [SerializeField] private GameObject visualbody;
        [SerializeField] private Rigidbody _rigidbody;

        public void HandleDeath()
        {
            if (deathFX != null)
            {
                deathFX.transform.parent = null;
                deathFX.transform.position = transform.position;
                deathFX.Play();
            }            
            visualbody.SetActive(false);
            _rigidbody.isKinematic = true;
        }

        public void ResetToDefault()
        {
            if (deathFX != null)
                deathFX.Stop();
            visualbody.SetActive(true);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.isKinematic = false;
        }
    }
}