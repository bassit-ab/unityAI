using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Centralised input dispatcher decoupled from UI components. Other gameplay systems can subscribe to its events without needing direct
    /// references to specific scene objects or UI pages.
    /// </summary>
    public class InputService : MonoBehaviour
    {
        private static InputService instance;
        public static InputService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InputService>();
                    if (instance == null)
                    {
                        // Create a new hidden GameObject if none exists
                        GameObject go = new GameObject("[InputService]");
                        instance = go.AddComponent<InputService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        public event SimpleVector2Callback OnPointerDragged;

        /// <summary>
        /// Invoked by UI components (e.g., InputHandler) to propagate drag input to listeners.
        /// </summary>
        public void RaisePointerDragged(Vector2 delta)
        {
            OnPointerDragged?.Invoke(delta);
        }
    }
}