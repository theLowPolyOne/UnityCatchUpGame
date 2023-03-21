using UnityEngine.AI;
using UnityEngine;

namespace Fallencake.CharacterController
{
    public class EnemyController : MonoBehaviour
    {
        #region FIELDS

        [Header("PARAMETERS:")]
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float atackDistance = 1.2f;

        [Header("COMPONENTS:")]
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform visualbody;

        private Vector3 targetPosition;

        public delegate void AttackEvent();
        public event AttackEvent OnAttack;

        #endregion

        #region MONO AND INITIALIZATION

        private void Update()
        {
            targetPosition = GameManager.Instance.Player.position;
            if (navMeshAgent.enabled) GoToTarget();
            if (Vector3.Distance(targetPosition, transform.position) < atackDistance)
            {
                OnAttack?.Invoke();
            }
        }

        #endregion

        public void StopMoving()
        {
            navMeshAgent.enabled = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.constraints = RigidbodyConstraints.None;
        }

        public void Warp(Vector3 position)
        {
            navMeshAgent.Warp(position);
        }

        private void GoToTarget()
        {
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(targetPosition);
        }

        public void ResetToDefault()
        {
            navMeshAgent.enabled = true;
            _rigidbody.isKinematic = true;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
        }
    }
}