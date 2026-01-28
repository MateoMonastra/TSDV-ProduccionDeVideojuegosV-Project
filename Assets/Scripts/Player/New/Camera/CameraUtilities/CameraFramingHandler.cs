using UnityEngine;

namespace Player.New
{
    public class CameraFramingHandler
    {
        private readonly MyCharacterCamera _camera;
        private Vector2 _defaultFraming = Vector2.zero;
        
        public CameraFramingHandler(MyCharacterCamera camera)
        {
            _camera = camera;
            _defaultFraming = _camera.followPointFraming;
        }

        public Vector3 ApplyFramingOffset(Vector3 position, Transform cameraTransform)
        {
            position += cameraTransform.right * _camera.followPointFraming.x;
            position += cameraTransform.up * _camera.followPointFraming.y;
            return position;
        }

        public void SetCameraFollowPointFraming(Vector2 framing)
        {
            _camera.followPointFraming = framing;
        }

        public void ResetCameraFollowPointFraming()
        {
            _camera.followPointFraming = _defaultFraming;
        }
    }
}