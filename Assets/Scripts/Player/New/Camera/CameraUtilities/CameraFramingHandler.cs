using UnityEngine;

namespace Player.New
{
    public class CameraFramingHandler
    {
        private readonly MyCharacterCamera _camera;

        public CameraFramingHandler(MyCharacterCamera camera)
        {
            _camera = camera;
        }

        public Vector3 ApplyFramingOffset(Vector3 position, Transform cameraTransform)
        {
            position += cameraTransform.right * _camera.followPointFraming.x;
            position += cameraTransform.up * _camera.followPointFraming.y;
            return position;
        }
    }
}