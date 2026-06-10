using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragynGames.Console
{
    internal class ConsoleWindowInputReader : IConsoleUserInput
    {
        private ConsoleAction consolePlayerInput;
        
        public void Bind(INeedConsoleInput consoleWindow)
        {
            consolePlayerInput = new ConsoleAction();
            consolePlayerInput.Enable();
            consolePlayerInput.ConsoleVisibility.Enable();
            consolePlayerInput.ConsoleVisibility.ToggleWindow.started += _ => consoleWindow.ToggleVisibility();
            consolePlayerInput.ConsoleControls.CycleHistoryUp.performed += _ => consoleWindow.CycleHistoryCommandUp();
            consolePlayerInput.ConsoleControls.CycleHistoryDown.performed += _ => consoleWindow.CycleHistoryCommandDown();
            consolePlayerInput.ConsoleControls.CycleSuggestionsUp.performed += _ => consoleWindow.CycleSuggestionsUp();
            consolePlayerInput.ConsoleControls.CycleSuggestionsDown.performed += _ => consoleWindow.CycleSuggestionsDown();
            //ToggelVisabilityAction = consoleWindow.ToggleVisibility;
            //CycleLastCommandUpAction = consoleWindow.CycleHistoryCommandUp;
            //CycleLastCommandDownAction = consoleWindow.CycleHistoryCommandDown;
            //CycleSuggestionUpAction = consoleWindow.CycleSuggestionsUp;
            //CycleSuggestionDownAction = consoleWindow.CycleSuggestionsDown;
            
            
        }
        public void DisableWindowToggle()
        {
            consolePlayerInput.ConsoleVisibility.ToggleWindow.Disable();
        }

        public void EnableWindowToggle()
        {
            consolePlayerInput.ConsoleVisibility.ToggleWindow.Enable();
        }

        public void EnableControls(bool shouldReadInput)
        {
                if (shouldReadInput)
                {
                    consolePlayerInput.ConsoleControls.Enable();
                }
                else
                {
                    consolePlayerInput.ConsoleControls.Disable();
                }
        }

        public Vector2 GetScreenPosition()
        {
            return Mouse.current.position.ReadValue();
        }
    }
}