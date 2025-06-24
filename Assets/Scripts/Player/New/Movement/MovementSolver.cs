using UnityEngine;

namespace Player.New
{
    public class MovementSolver
    {
        private readonly CapsuleCollider _capsule;
        private readonly LayerMask _collidableLayers;
        private readonly float _collisionOffset = 0.01f;
        private readonly int _maxMovementIterations = 5;
        private readonly RigidbodyInteractionHandler _rigidbodyHandler;

        public MovementSolver(CapsuleCollider capsule, LayerMask collidableLayers,
            RigidbodyInteractionHandler rigidbodyHandler)
        {
            _capsule = capsule;
            _collidableLayers = collidableLayers;
            _rigidbodyHandler = rigidbodyHandler;
        }

        public bool Solve(ref Vector3 velocity, float deltaTime, ref Vector3 position)
        {
            if (deltaTime <= 0f || velocity.sqrMagnitude == 0f) return false;

            Vector3 direction = velocity.normalized;
            float remainingDistance = velocity.magnitude * deltaTime;
            int iterations = 0;
            bool hitSomething = false;

            while (remainingDistance > 0f && iterations < _maxMovementIterations)
            {
                if (Physics.CapsuleCast(
                        GetCapsuleBottom(position),
                        GetCapsuleTop(position),
                        _capsule.radius,
                        direction,
                        out RaycastHit hit,
                        remainingDistance + _collisionOffset,
                        _collidableLayers))
                {
                    float moveDist = Mathf.Max(0f, hit.distance - _collisionOffset);
                    position += direction * moveDist;
                    remainingDistance -= moveDist;

                    Rigidbody rb = hit.collider.attachedRigidbody;
                    if (rb != null && !rb.isKinematic)
                    {
                        RigidbodyProjectionHit projHit = new RigidbodyProjectionHit
                        {
                            Rigidbody = rb,
                            Point = hit.point,
                            Normal = hit.normal
                        };
                        _rigidbodyHandler.HandleInteraction(ref velocity, projHit, deltaTime);
                    }

                    direction = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
                    velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                    hitSomething = true;
                }
                else
                {
                    position += direction * remainingDistance;
                    remainingDistance = 0f;
                }

                iterations++;
            }

            return hitSomething;
        }

        private Vector3 GetCapsuleBottom(Vector3 position)
        {
            return position + _capsule.center + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
        }

        private Vector3 GetCapsuleTop(Vector3 position)
        {
            return position + _capsule.center + Vector3.up * (_capsule.height * 0.5f - _capsule.radius);
        }
    }
}