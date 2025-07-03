using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "Game/Gameplay Settings")]
    public class GameplaySettings : ScriptableObject
    {
        [Header("Player Movement")]
        public float dragSensitivity = 1f;
        public float defaultForwardSpeed = 6f;
        public float laneWidth = 2f;
        public float jumpHeight = 1.2f;

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 5f, -10f);

        // Add more tunables here as needed…
    }
}