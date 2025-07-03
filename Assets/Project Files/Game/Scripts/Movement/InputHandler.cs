using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class InputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] float dragSensitivity = 1f;

        private Vector2 pointerPosition;

        private GameplaySettings gameplaySettings;

        public bool IsPointerDown { get; private set; }
        public Vector2 DragDirection { get; private set; }

        public event SimpleVector2Callback OnPointerDragged;

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPointerDown = true;

            pointerPosition = eventData.position;

            // Ensure drag sensitivity synced with settings
            if(gameplaySettings == null)
                gameplaySettings = Resources.Load<GameplaySettings>("GameplaySettings");

            if(gameplaySettings != null)
                dragSensitivity = gameplaySettings.dragSensitivity;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Normalize drag distance based on screen dimensions
            float normalizedDragX = eventData.delta.x / Screen.width;
            float normalizedDragY = eventData.delta.y / Screen.height;

            // Calculate drag direction
            DragDirection = new Vector2(normalizedDragX, normalizedDragY) * dragSensitivity;

            OnPointerDragged?.Invoke(DragDirection);
            // Broadcast to global InputService so other systems can listen without a direct UI reference
            InputService.Instance.RaisePointerDragged(DragDirection);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPointerDown = false;
        }
    }
}