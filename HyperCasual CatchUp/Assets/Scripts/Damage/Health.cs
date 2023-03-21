using UnityEngine;

namespace Fallencake.Damage
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _Health;
        [SerializeField] private int _MaxHealth = 100;
        public int CurrentHealth { get => _Health; private set => _Health = value; }
        public int MaxHealth { get => _MaxHealth; private set => _MaxHealth = value; }

        public event IDamageable.TakeDamageEvent OnTakeDamage;
        public event IDamageable.DeathEvent OnDeath;

        private void OnEnable()
        {
            ResetToDefault();
        }

        public void ResetToDefault()
        {
            _Health = MaxHealth;
        }

        public void TakeDamage(int Damage)
        {
            int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);

            CurrentHealth -= damageTaken;

            if (damageTaken != 0)
            {
                OnTakeDamage?.Invoke(damageTaken);
            }

            if (CurrentHealth == 0 && damageTaken != 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}