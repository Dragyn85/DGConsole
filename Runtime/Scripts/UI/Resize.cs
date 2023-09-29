using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragynGames
{
    public class Resize : MonoBehaviour,IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform targetWindow;

        private Vector3 windowHoldOffset;
        private bool isWindowGrabbed;

        private Vector2 startPosition;
        private Vector2 startSize;

        public void OnPointerDown(PointerEventData eventData)
        {
            startPosition = eventData.position;
            startSize = targetWindow.sizeDelta;
            isWindowGrabbed = true;
            StartCoroutine(MoveWindow());
        }

        private IEnumerator MoveWindow() {
            while(isWindowGrabbed)
            {
                Vector2 mouseDelta = startPosition - (Vector2)Input.mousePosition;
                targetWindow.sizeDelta = new Vector2(startSize.x - mouseDelta.x,startSize.y+mouseDelta.y);
                yield return null;
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            isWindowGrabbed = false;
        }
    }
}
