using UnityEngine;

namespace VaultSurvivors.Environment
{
    public class DroneHover : MonoBehaviour
    {
        [Header("Hover Settings")]
        [Tooltip("The distance and direction the drone will travel from its starting position. E.g., Y=2 means it will go up 2 units and back down.")]
        public Vector3 movementDistance = new Vector3(0f, 2f, 0f);
        
        [Tooltip("How fast the drone moves back and forth.")]
        public float speed = 2f;

        [Tooltip("Offset the start time so multiple drones don't all move perfectly synchronized.")]
        public float timeOffset = 0f;

        private Vector3 _startLocalPos;

        private void Start()
        {
            // Remember where we were placed in the scene
            _startLocalPos = transform.localPosition;
        }

        private void Update()
        {
            // Use a Sine wave to create a perfectly smooth ease-in/ease-out oscillation.
            // Mathf.Sin goes from -1 to 1. We map it to 0 to 1 for the Lerp.
            float t = (Mathf.Sin((Time.time + timeOffset) * speed) + 1f) / 2f;
            
            // Smoothly move back and forth between the start position and the target distance
            transform.localPosition = Vector3.Lerp(_startLocalPos, _startLocalPos + movementDistance, t);
        }
    }
}
