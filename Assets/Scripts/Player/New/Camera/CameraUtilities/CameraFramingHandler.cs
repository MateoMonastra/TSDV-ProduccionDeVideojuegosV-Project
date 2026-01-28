using UnityEngine;

namespace Player.New
{
    public class CameraFramingHandler
    {
        private readonly MyCharacterCamera _camera;
        public Vector2 DefaultFraming { get; }

        public CameraFramingHandler(MyCharacterCamera camera)
        {
            _camera = camera;
            DefaultFraming = _camera.followPointFraming;
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
            _camera.followPointFraming = DefaultFraming;
        }
        
    }
}