using UnityEngine;

namespace Platforms
{
    public class PingPongPlatform : MonoBehaviour
    {
        public enum Direction { Horizontal, Vertical, Both }

        public Direction movementDirection = Direction.Horizontal;
        public float distance;
        public float speed;

        private Vector3 _startPoint;

        void Start()
        {
            _startPoint = transform.position;
        }

        void Update()
        {
            float offsetX = 0f;
            float offsetY = 0f;

            if (movementDirection == Direction.Horizontal || movementDirection == Direction.Both)
            {
                offsetX = Mathf.PingPong(Time.time * speed, distance * 2) - distance;
            }

            if (movementDirection == Direction.Vertical || movementDirection == Direction.Both)
            {
                offsetY = Mathf.PingPong(Time.time * speed, distance * 2) - distance;
            }

            transform.position = _startPoint + new Vector3(offsetX, offsetY, 0);
        }
    }
}
