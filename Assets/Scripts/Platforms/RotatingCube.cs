using UnityEngine;

namespace Platforms
{
    public class RotatingCube : MonoBehaviour
    {
        public float rotationSpeed;

        void Update()
        {
            
            transform.Rotate(Vector3.right * (rotationSpeed * Time.deltaTime));
        }
    }
}