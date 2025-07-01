using UnityEngine;

namespace Player.New
{
    public struct CharacterGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;
        public Collider GroundCollider;
        public Vector3 GroundPoint;

        public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
        {
            FoundAnyGround = transientGroundingReport.FoundAnyGround;
            IsStableOnGround = transientGroundingReport.IsStableOnGround;
            SnappingPrevented = transientGroundingReport.SnappingPrevented;
            GroundNormal = transientGroundingReport.GroundNormal;
            InnerGroundNormal = transientGroundingReport.InnerGroundNormal;
            OuterGroundNormal = transientGroundingReport.OuterGroundNormal;
            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
    }

    public struct CharacterTransientGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            FoundAnyGround = groundingReport.FoundAnyGround;
            IsStableOnGround = groundingReport.IsStableOnGround;
            SnappingPrevented = groundingReport.SnappingPrevented;
            GroundNormal = groundingReport.GroundNormal;
            InnerGroundNormal = groundingReport.InnerGroundNormal;
            OuterGroundNormal = groundingReport.OuterGroundNormal;
        }
    }
}