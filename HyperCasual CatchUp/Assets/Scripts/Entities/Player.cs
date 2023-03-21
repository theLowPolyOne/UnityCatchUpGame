using Fallencake.CharacterController;
using Fallencake.Damage;
using UnityEngine;

namespace Fallencake.Entities.Characters
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        public Health Health;
        public PlayerController Movement;
        public PainResponse PainResponse;
        [SerializeField] private GameObject swayingCube;

        private Vector3 defaultPosition;
        private Vector3 defaultCubePosition;

        private void Start()
        {
            defaultPosition = transform.position;
            defaultCubePosition = swayingCube.transform.position;
            Health.OnDeath += Die;
            Movement.TakeAwayControl();
        }

        private void Die()
        {
            Movement.TakeAwayControl();
            PainResponse.HandleDeath();
        }

        public void ResetToDefault()
        {
            transform.position = defaultPosition;
            swayingCube.transform.position = defaultCubePosition;
            Health.ResetToDefault();
            PainResponse.ResetToDefault();
            Movement.ReturnControl();
        }
    }
}