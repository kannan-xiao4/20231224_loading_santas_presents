using UnityEngine;

namespace Assets.Scripts
{
    public class MovingSledge : MonoBehaviour
    {
        public float speed = 2.0f;
        public Vector3Int distance = Vector3Int.one;

        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
        }

        private void FixedUpdate()
        {
            MovePlatform();
        }

        private void MovePlatform()
        {
            var timeDiff = Time.fixedTime * speed;
            var newX = distance.x == 0 ? 0 : Mathf.PingPong(timeDiff, Mathf.Abs(distance.x)) * Mathf.Sign(distance.x);
            var newY = distance.y == 0 ? 0 : Mathf.PingPong(timeDiff, Mathf.Abs(distance.y)) * Mathf.Sign(distance.y);
            var newZ = distance.z == 0 ? 0 : Mathf.PingPong(timeDiff, Mathf.Abs(distance.z)) * Mathf.Sign(distance.z);
            transform.position = initialPosition + new Vector3(newX, newY, newZ);
        }
    }

}