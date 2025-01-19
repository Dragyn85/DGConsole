using System;
using UnityEngine;

namespace DragynGames.Console
{
    internal class ObjectMouseSelection : MonoBehaviour, IConsoleComponent
    {
        [SerializeField] private Material highlightMaterial;
        private HighLighting currentHighlight;
        private ConsoleWindow consoleWindow;

        public void Tick(float deltatime)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
            {
                HandleHighlight();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (currentHighlight != null)
                {
                    currentHighlight.RemoveHighlight();
                }

                currentHighlight = null;
            }
        }
        
        private void HandleHighlight()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (currentHighlight == null)
                {
                    currentHighlight = hitObject.AddComponent<HighLighting>();
                    currentHighlight.ActivateHighlight(highlightMaterial, true);
                }
                else if (currentHighlight.gameObject != hitObject)
                {
                    currentHighlight.RemoveHighlight();
                    currentHighlight = hitObject.AddComponent<HighLighting>();
                    currentHighlight.ActivateHighlight(highlightMaterial, true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (currentHighlight != null)
                    {
                        currentHighlight.HighlightPulse(highlightMaterial, true);
                    }

                    consoleWindow.CurrentTarget.Set(hitObject);
                    consoleWindow.ActivateInputMode();
                }
            }
            else
            {
                if (currentHighlight != null)
                {
                    currentHighlight.RemoveHighlight();
                }

                currentHighlight = null;
            }
        }

        public void OnConsoleWindowAttached(ConsoleWindow consoleWindow)
        {
            this.consoleWindow = consoleWindow;
        }
    }
}