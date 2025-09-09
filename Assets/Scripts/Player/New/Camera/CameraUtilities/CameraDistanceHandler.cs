namespace Player.New
{
    public class CameraDistanceHandler
    {
        private readonly MyCharacterCamera _camera;

        public float TargetDistance { get; private set; }

        public CameraDistanceHandler(MyCharacterCamera camera)
        {
            _camera = camera;
            TargetDistance = camera.defaultDistance;
        }

        public void ProcessZoomInput(float zoomInput)
        {
            TargetDistance += zoomInput * _camera.distanceMovementSpeed;
            TargetDistance = UnityEngine.Mathf.Clamp(
                TargetDistance,
                _camera.minDistance,
                _camera.maxDistance
            );
        }
    }
}