using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragynGames.Commands.UI
{
    internal class Move : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Transform targetWindow;

        private Vector3 windowHoldOffset;
        private bool isWindowGrabbed;

        public void OnPointerDown(PointerEventData eventData) {
            windowHoldOffset = targetWindow.position - (Vector3)eventData.position;
            isWindowGrabbed = true;
            StartCoroutine(MoveWindow());
        }

        private IEnumerator MoveWindow() {
            while(isWindowGrabbed) {
                targetWindow.position = Input.mousePosition + windowHoldOffset;
                yield return null;
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            isWindowGrabbed = false;
        }
    }
}
