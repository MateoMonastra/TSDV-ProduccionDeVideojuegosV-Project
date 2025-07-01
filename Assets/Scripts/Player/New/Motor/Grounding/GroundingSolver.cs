using UnityEngine;

namespace Player.New
{
    public class GroundingSolver
    {
        private readonly CapsuleCollider _capsule;
        private readonly float _minGroundDot = 0.7f;
        private readonly float _maxStepHeight = 0.4f;
        private readonly float _stepCheckForwardDistance = 0.1f;
        private readonly float _stepCheckDownDistance = 0.5f;
        private readonly LayerMask _groundLayers;

        public GroundingSolver(CapsuleCollider capsule, LayerMask groundLayers)
        {
            _capsule = capsule;
            _groundLayers = groundLayers;
        }

        public void CheckProbe(ref Vector3 position, Quaternion rotation, float probeDistance, Vector3 baseVelocity,
            ref CharacterGroundingReport groundingReport)
        {
            Vector3 capsuleBottom = position + _capsule.center + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
            float castDistance = probeDistance + _capsule.radius;
            
            if (Physics.SphereCast(capsuleBottom, _capsule.radius * 0.9f, Vector3.down, out RaycastHit hit, 
                    castDistance, _groundLayers))
            {
                groundingReport.FoundAnyGround = true;
                groundingReport.GroundNormal = hit.normal;
                groundingReport.GroundPoint = hit.point;
                groundingReport.GroundCollider = hit.collider;
                groundingReport.IsStableOnGround = IsStable(hit.normal);
                
                if (groundingReport.IsStableOnGround)
                {
                    EvaluateEdgeFeatures(hit, ref groundingReport);
                }
                else
                {
                    groundingReport.SnappingPrevented = true;
                }
                
                if (!groundingReport.IsStableOnGround && groundingReport.FoundAnyGround)
                {
                    TryStepUp(ref position, rotation, baseVelocity, ref groundingReport);
                }
            }
            else
            {
                groundingReport = new CharacterGroundingReport();
            }
        }

        public bool IsStable(Vector3 normal)
        {
            return Vector3.Dot(normal, Vector3.up) >= _minGroundDot;
        }

        private void EvaluateEdgeFeatures(RaycastHit groundHit, ref CharacterGroundingReport report)
        {
            Vector3 edgeTestDir = Vector3.Cross(groundHit.normal, Vector3.up).normalized;
            if (edgeTestDir != Vector3.zero)
            {
                Vector3 sideRayOrigin = groundHit.point + edgeTestDir * _capsule.radius * 0.5f + Vector3.up * 0.05f;
                if (!Physics.Raycast(sideRayOrigin, Vector3.down, out RaycastHit edgeHit, 0.2f, _groundLayers))
                {
                    report.SnappingPrevented = true;
                }
            }
        }

        private void TryStepUp(ref Vector3 position, Quaternion rotation, Vector3 baseVelocity,
            ref CharacterGroundingReport report)
        {
            Vector3 forward = rotation * Vector3.forward;
            Vector3 origin = position + _capsule.center + Vector3.up * _maxStepHeight * 0.5f;
            Vector3 stepRayStart = origin;
            Vector3 stepRayDir = forward;

            if (Physics.Raycast(stepRayStart, stepRayDir, out RaycastHit forwardHit, _stepCheckForwardDistance,
                    _groundLayers))
            {
                Vector3 downRayOrigin = forwardHit.point + Vector3.up * (_maxStepHeight * 0.5f);

                if (Physics.Raycast(downRayOrigin, Vector3.down, out RaycastHit downHit, _stepCheckDownDistance,
                        _groundLayers))
                {
                    float verticalOffset = downHit.point.y - position.y;
                    if (verticalOffset <= _maxStepHeight)
                    {
                        position = new Vector3(position.x, downHit.point.y, position.z);
                        report.FoundAnyGround = true;
                        report.IsStableOnGround = true;
                        report.GroundPoint = downHit.point;
                        report.GroundNormal = downHit.normal;
                        report.GroundCollider = downHit.collider;
                    }
                }
            }
        }
        
        public void DrawGizmos(Vector3 position, Quaternion rotation)
        {
            Debug.Log("Dibujando gizmos");
            Vector3 origin = position + _capsule.center;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, _capsule.radius);
            Gizmos.DrawLine(origin, origin + Vector3.down * 0.3f);

            Vector3 stepStart = origin + Vector3.up * (_maxStepHeight * 0.5f);
            Vector3 forward = rotation * Vector3.forward;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(stepStart, stepStart + forward * _stepCheckForwardDistance);
        }
    }
}