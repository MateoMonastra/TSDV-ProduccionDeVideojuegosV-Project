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

        public float GetAdjustedDistance(Vector3 origin, Quaternion rotation, float deltaTime)
        {
            float targetDistance = _distanceHandler.TargetDistance;
            RaycastHit closestHit = new RaycastHit { distance = Mathf.Infinity };
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                _camera.obstructionCheckRadius,
                -(rotation * Vector3.forward),
                _obstructions,
                targetDistance,
                _camera.obstructionLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (IsIgnored(_obstructions[i].collider)) continue;
                if (_obstructions[i].distance < closestHit.distance && _obstructions[i].distance > 0)
                {
                    closestHit = _obstructions[i];
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