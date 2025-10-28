using Unity.Mathematics;
using UnityEngine;

namespace Player.New
{
    public class MovementSolver
    {
        private readonly CapsuleCollider _capsule;
        private readonly LayerMask _collidableLayers;
        private readonly int _maxMovementIterations = 12;
        private readonly RigidbodyInteractionHandler _rigidbodyHandler;
        private readonly Collider[] _overlaps = new Collider[16];
        private readonly int _initialOverlapSolvePasses = 2;
        private const float Skin = 0.02f;
        private const float Backoff = 0.0025f;
        private const float MinVelocity = 1e-6f;

        public MovementSolver(CapsuleCollider capsule, LayerMask collidableLayers,
            RigidbodyInteractionHandler rigidbodyHandler)
        {
            _capsule = capsule;
            _collidableLayers = collidableLayers;
            _rigidbodyHandler = rigidbodyHandler;
        }

        public bool Solve(ref Vector3 velocity, float deltaTime, ref Vector3 position)
        {
            if (deltaTime <= 0f || velocity.sqrMagnitude == 0f)
                return false;
            
            SolveOverlaps(ref position, Quaternion.identity, _initialOverlapSolvePasses, _collidableLayers, _capsule);
            
            float remaining = velocity.magnitude * deltaTime;
            Vector3 dir = velocity.normalized;

            int iterations = 0;
            bool hitSomething = false;
            
            float maxStep = Mathf.Max(0.25f * _capsule.radius, 0.05f);

            while (remaining > MinVelocity && iterations < _maxMovementIterations)
            {
                float step = Mathf.Min(remaining, maxStep);
                
                if (Physics.CapsuleCast(
                        GetCapsuleBottom(position),
                        GetCapsuleTop(position),
                        _capsule.radius,
                        dir,
                        out RaycastHit hit,
                        step + Skin,
                        _collidableLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    float moveDist = Mathf.Max(0f, hit.distance - Skin);
                    position += dir * moveDist;
                    
                    position += hit.normal * Backoff;
                    
                    velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                    dir = velocity.normalized;
                    
                    remaining -= moveDist;

                    hitSomething = true;
                    
                    SolveOverlaps(ref position, Quaternion.identity, 1, _collidableLayers, _capsule);
                    
                    if (velocity.sqrMagnitude < MinVelocity)
                        break;
                }
                else
                {
                    position += dir * step;
                    remaining -= step;
                    
                    SolveOverlaps(ref position, Quaternion.identity, 1, _collidableLayers, _capsule);
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


        private void SolveOverlaps(ref Vector3 position, Quaternion rotation, int passes, int layers,
            CapsuleCollider cap)
        {
            for (int p = 0; p < passes; p++)
            {
                bool moved = false;

                Vector3 bottom = position + cap.center + Vector3.down * (cap.height * 0.5f - cap.radius);
                Vector3 top = position + cap.center + Vector3.up * (cap.height * 0.5f - cap.radius);

                int count = Physics.OverlapCapsuleNonAlloc(
                    bottom, top, cap.radius, _overlaps, layers, QueryTriggerInteraction.Ignore);

                for (int i = 0; i < count; i++)
                {
                    var other = _overlaps[i];
                    if (!other || other == cap) continue;

                    if (Physics.ComputePenetration(
                            cap, position, rotation,
                            other, other.transform.position, other.transform.rotation,
                            out Vector3 dir, out float dist))
                    {
                        position += dir * (dist + 0.001f);
                        moved = true;
                    }
                }

                if (!moved) break;
            }
        }
    }
}