using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class EventSystemCleaner : MonoBehaviour
    {
        void Awake()
        {
            var systems = FindObjectsOfType<EventSystem>();
            if (systems.Length > 1)
            {
                for (int i = 1; i < systems.Length; i++)
                {
                    Destroy(systems[i].gameObject);
                }
            }
        }
    }
}