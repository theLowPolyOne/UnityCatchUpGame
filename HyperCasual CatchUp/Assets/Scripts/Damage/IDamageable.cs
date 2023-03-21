using UnityEngine;

namespace Fallencake.Damage
{
    public interface IDamageable
    {
        public int CurrentHealth { get; }
        public int MaxHealth { get; }

        public delegate void TakeDamageEvent(int Damage);
        public event TakeDamageEvent OnTakeDamage;

        public delegate void DeathEvent();
        public event DeathEvent OnDeath;

        public void TakeDamage(int Damage);
    }
}