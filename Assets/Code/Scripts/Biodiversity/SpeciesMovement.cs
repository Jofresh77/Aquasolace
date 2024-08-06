using System.Collections.Generic;
using Code.Scripts.Structs;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Scripts.Biodiversity
{
    public class SpeciesMovement : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private float idleTime = 2f;
        [SerializeField] private float movementDuration = 1f;
        [SerializeField] private AnimationCurve velocityCurve;

        private Animator _animator;
        private static readonly int AnimSpeed = Animator.StringToHash("animSpeed");
        private List<Coordinate> _clusterCoordinates;
        private Coroutine _movementCoroutine;

        private enum MovementState
        {
            Moving,
            Idle
        }

        private MovementState _currentState = MovementState.Idle;

        public void Initialize(List<Coordinate> clusterCoordinates)
        {
            _clusterCoordinates = clusterCoordinates;
            InitializeComponents();
            StartMovement();
        }

        private void InitializeComponents()
        {
            _animator = GetComponentInChildren<Animator>();
            _animator.SetFloat(AnimSpeed, 0.0f);
        }

        private void StartMovement()
        {
            if (TryGetRandomPoint(out Vector3 destination))
            {
                SetDestination(destination);
            }
            else
            {
                Debug.LogError("Failed to find initial movement point");
            }
        }

        private void Update()
        {
            UpdateAnimationSpeed();
            UpdateMovementState();
        }

        private void UpdateAnimationSpeed()
        {
            float speed = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
            _animator.SetFloat(AnimSpeed, speed);
        }

        private void UpdateMovementState()
        {
            switch (_currentState)
            {
                case MovementState.Moving when HasReachedDestination():
                    TransitionToIdle();
                    break;
                case MovementState.Idle when ShouldResumeMoving():
                    TransitionToMoving();
                    break;
            }
        }

        private bool HasReachedDestination()
        {
            return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
        }

        private bool ShouldResumeMoving()
        {
            return navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;
        }

        private void TransitionToIdle()
        {
            _currentState = MovementState.Idle;
            StopAgent();
            _movementCoroutine = StartCoroutine(IdleRoutine());
        }

        private void TransitionToMoving()
        {
            _currentState = MovementState.Moving;
            navMeshAgent.isStopped = false;
            StartCoroutine(ApplyVelocityCurve());
        }

        private void StopAgent()
        {
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
            }

            navMeshAgent.isStopped = true;
        }

        private System.Collections.IEnumerator IdleRoutine()
        {
            yield return new WaitForSeconds(idleTime);

            if (TryGetRandomPoint(out Vector3 newDestination))
            {
                SetDestination(newDestination);
            }
            else
            {
                Debug.LogError("Failed to find new movement point");
            }
        }

        private void SetDestination(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);
            TransitionToMoving();
        }

        private System.Collections.IEnumerator ApplyVelocityCurve()
        {
            float elapsedTime = 0f;
            float initialSpeed = navMeshAgent.speed;

            while (elapsedTime < movementDuration && _currentState == MovementState.Moving)
            {
                float t = elapsedTime / movementDuration;
                float curveValue = velocityCurve.Evaluate(t);
                navMeshAgent.speed = initialSpeed * curveValue;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            navMeshAgent.speed = initialSpeed;
        }

        private bool TryGetRandomPoint(out Vector3 result)
        {
            if (_clusterCoordinates.Count == 0)
            {
                result = Vector3.zero;
                return false;
            }

            var randomCoordinate = _clusterCoordinates[Random.Range(0, _clusterCoordinates.Count)];
            result = new Vector3(randomCoordinate.X, 0, randomCoordinate.Z);
            return true;
        }
    }
}