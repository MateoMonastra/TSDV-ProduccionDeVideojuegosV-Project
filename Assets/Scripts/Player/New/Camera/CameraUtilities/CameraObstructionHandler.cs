using UnityEngine;

namespace Player.New
{
    public class CameraObstructionHandler
    {
        private readonly MyCharacterCamera _camera;
        private readonly CameraDistanceHandler _distanceHandler;
        private const int MaxObstructions = 32;
        private readonly RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];

        private float _currentDistance;

        public CameraObstructionHandler(MyCharacterCamera camera, CameraDistanceHandler distanceHandler)
        {
            _camera = camera;
            _distanceHandler = distanceHandler;
            _currentDistance = _distanceHandler.TargetDistance;
        }

        public float GetAdjustedDistance(Vector3 origin, Vector3 desiredCameraPosition, float deltaTime)
        {
            Vector3 toDesired = desiredCameraPosition - origin;
            float targetDistance = toDesired.magnitude;

            RaycastHit closestHit = new RaycastHit { distance = Mathf.Infinity };

            if (targetDistance > 0.0001f)
            {
                Vector3 dir = toDesired / targetDistance;

                int hitCount = Physics.SphereCastNonAlloc(
                    origin,
                    _camera.obstructionCheckRadius,
                    dir,
                    _obstructions,
                    targetDistance,
                    _camera.obstructionLayers,
                    QueryTriggerInteraction.Ignore);

                for (int i = 0; i < hitCount; i++)
                {
                    if (IsIgnored(_obstructions[i].collider)) continue;
                    if (_obstructions[i].distance < closestHit.distance && _obstructions[i].distance >= 0f)
                    {
                        closestHit = _obstructions[i];
                    }
                }
            }

            if (closestHit.distance < Mathf.Infinity)
            {
                _currentDistance = Mathf.Lerp(
                    _currentDistance,
                    closestHit.distance,
                    1f - Mathf.Exp(-_camera.obstructionSharpness * deltaTime));
            }
            else
            {
                _currentDistance = Mathf.Lerp(
                    _currentDistance,
                    targetDistance,
                    1f - Mathf.Exp(-_camera.distanceMovementSharpness * deltaTime));
            }

            return _currentDistance;
        }

        private bool IsIgnored(Collider col)
        {
            foreach (var ignored in _camera.ignoredColliders)
            {
                if (ignored == col) return true;
            }
            return false;
        }
    }
}