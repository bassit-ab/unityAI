using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CharacterSelector : MonoBehaviour
    {
        [Header("Preview Parent")]
        [SerializeField] private Transform previewParent;

        [Header("Characters List")]
        [SerializeField] private List<GameObject> characters = new List<GameObject>();
        [SerializeField] private int startIndex = 0;
        [SerializeField] private float swipeThreshold = 0.2f; // normalized drag X to switch characters

        private GameObject activePreview;
        private int currentIndex = 0;

        private void Awake()
        {
            // Subscribe to global input service for swipe gestures
            InputService.Instance.OnPointerDragged += OnPointerDragged;
        }

        private void Start()
        {
            if (characters.Count == 0)
                return;

            currentIndex = Mathf.Clamp(startIndex, 0, characters.Count - 1);
            SpawnCharacter(characters[currentIndex]);
        }

        private void OnDestroy()
        {
            if (InputService.Instance != null)
                InputService.Instance.OnPointerDragged -= OnPointerDragged;
        }

        private void OnPointerDragged(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) < swipeThreshold)
                return;

            if (delta.x > 0)
                ShowPrevious();
            else
                ShowNext();
        }

        private void ShowNext()
        {
            if (characters.Count == 0)
                return;

            currentIndex = (currentIndex + 1) % characters.Count;
            SpawnCharacter(characters[currentIndex]);
        }

        private void ShowPrevious()
        {
            if (characters.Count == 0)
                return;

            currentIndex--;
            if (currentIndex < 0) currentIndex = characters.Count - 1;
            SpawnCharacter(characters[currentIndex]);
        }

        public void SelectCharacter(GameObject prefab)
        {
            SpawnCharacter(prefab);
        }

        private void SpawnCharacter(GameObject prefab)
        {
            if (activePreview != null)
                Destroy(activePreview);

            if (prefab == null) return;

            activePreview = Instantiate(prefab, previewParent);
            var t = activePreview.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one * 100;

            foreach (var rb in activePreview.GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = true;
        }
    }
}