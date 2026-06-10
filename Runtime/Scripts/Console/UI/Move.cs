using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragynGames.Console.UI
{
    internal class Move : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IUseScreenPosition
    {
        [SerializeField] private Transform targetWindow;
        
        
        IConsoleUserInput _screenPositionProvider;
        private Vector2 windowHoldOffset;
        private bool isWindowGrabbed;
        
        public event Action OnMoveFinished;

        public void OnPointerDown(PointerEventData eventData) {
            windowHoldOffset = targetWindow.position - (Vector3)eventData.position;
            isWindowGrabbed = true;
            StartCoroutine(MoveWindow());
        }

        private IEnumerator MoveWindow() {
            while(isWindowGrabbed) {
                targetWindow.position = _screenPositionProvider.GetScreenPosition() + windowHoldOffset;
                yield return null;
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            isWindowGrabbed = false;
            OnMoveFinished?.Invoke();
        }

        public void SetInputReader(IConsoleUserInput screenPositionProvider)
        {
            _screenPositionProvider = screenPositionProvider;
        }
    }
}
