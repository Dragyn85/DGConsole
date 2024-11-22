using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Screen = UnityEngine.Screen;
using Action = System.Action;

namespace DragynGames.Console.UI
{
    internal class Resize : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform targetWindow;
        [SerializeField] private float responsivenessMultiplier = 1.8f; // Adjustable multiplier for responsiveness

        private bool isWindowGrabbed;

        private Vector2 startPosition;
        private Vector2 startSize;

        public event Action OnResizeFinished;
        public void OnPointerDown(PointerEventData eventData)
        {
            startPosition = eventData.position;
            startSize = targetWindow.sizeDelta;
            isWindowGrabbed = true;
            StartCoroutine(ResizeWindow());
        }

        private IEnumerator ResizeWindow() {
            while(isWindowGrabbed)
            {
                Vector2 mouseDelta = startPosition - (Vector2)Input.mousePosition;

                // Apply scaling based on screen resolution
                Vector2 scaledDelta = ScaleDelta(mouseDelta);

                targetWindow.sizeDelta = new Vector2(startSize.x - scaledDelta.x, startSize.y + scaledDelta.y);
                yield return null;
            }
        }

        private Vector2 ScaleDelta(Vector2 delta) {
            float scaleFactor = CalculateScaleFactor();
            return delta * scaleFactor;
        }

        private float CalculateScaleFactor() {
            float standardResolution = 1080f; // Standard resolution for scaling (1080p)
            float maxScreenDimension = Mathf.Max(Screen.width, Screen.height);
            return (standardResolution / maxScreenDimension) * responsivenessMultiplier;
        }

        public void OnPointerUp(PointerEventData eventData) {
            isWindowGrabbed = false;
            OnResizeFinished?.Invoke();
        }
    }
}