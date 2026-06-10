using System;
using DragynGames.Console;
using UnityEngine;

namespace DragynGames
{
    internal class ConsoleWindowLegacyInputReader : MonoBehaviour, IConsoleUserInput
    {
        [SerializeField] KeyCode toggleVisabilityKey = KeyCode.BackQuote;
        private Action cycleHistoryDown;
        private Action cycleHistotyUp;
        private Action cycleSuggestionDown;
        private Action cycleSuggestionUp;
        private Action toggleVisability;

        private bool _isEnabled;
        private void Update()
        {
            if (toggleVisability != null && Input.GetKeyDown(toggleVisabilityKey))
            {
                toggleVisability.Invoke();
            }
            if (!_isEnabled) return;
            
            
            if (Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftControl))
            {
                cycleSuggestionUp?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftControl))
            {
                cycleSuggestionDown?.Invoke();
            }
            if(Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftControl))            {
                cycleHistotyUp?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftControl))
            {
                cycleHistoryDown?.Invoke();
            }

        }
        public Vector2 GetScreenPosition()
        {
            return Input.mousePosition;
        }

        public void Bind(INeedConsoleInput consoleWindow)
        {
            cycleHistoryDown = consoleWindow.CycleHistoryCommandDown;
            cycleHistotyUp = consoleWindow.CycleSuggestionsUp;
            cycleSuggestionDown = consoleWindow.CycleSuggestionsDown;
            cycleSuggestionUp = consoleWindow.CycleSuggestionsUp;
            toggleVisability = consoleWindow.ToggleVisibility;
        }

        public void EnableControls(bool shouldReadInput)
        {
            _isEnabled = shouldReadInput;
        }
    }
}
