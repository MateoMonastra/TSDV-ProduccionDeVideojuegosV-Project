using UnityEngine;

namespace Player.New
{
    public class RigidbodyInteractionHandler
    {
        private float _characterMass;
        public RigidbodyInteractionHandler(float characterMass) => _characterMass = characterMass;

        public void HandleInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
        {
            if (hit.Rigidbody == null || hit.Rigidbody.isKinematic)
                return;

            float bodyMass = hit.Rigidbody.mass;
            float massRatio = _characterMass / (_characterMass + bodyMass);
            Vector3 impulse = processedVelocity * massRatio;

            hit.Rigidbody.AddForceAtPosition(impulse, hit.Point, ForceMode.VelocityChange);
        }
    }

    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 Point;
        public Vector3 Normal;
    }
}