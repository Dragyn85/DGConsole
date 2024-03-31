using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragynGames.Commands.UI
{
    internal class ConsoleWindow : MonoBehaviour
    {
        [Header("Visibility")]
        [SerializeField] KeyCode toggleVisabilty = KeyCode.L;
        [SerializeField, Range(0, 1)] float visbleAlpha = 1;



        [Space(10)]
        [Header("Message area")]
        [SerializeField] Transform windowContent;
        [SerializeField] TMP_Text messagePrefab;

        [Space(10)]
        [Header("Commands")]
        [SerializeField] char[] commandPrefix;
        [SerializeField] Transform commandTipArea;

        TMP_InputField inputField;
        [SerializeField] int maxNumberOfTips = 5;
        [SerializeField] bool visible;
        private CanvasGroup canvasGroup;

        [SerializeField] Assembly[] assembly;
        CommandManager commandManager;
        List<TMP_Text> inputTexts = new();
        
        
        private Queue<string> lastInputs = new();
        private int selectedLastCommand;
        private int maxStoredInputs = 10;
        private string currentInput;

        private ConsoleSettings _settings;
        
        private void Awake()
        {
            _settings = new ConsoleSettings(inputTexts);
            commandManager = new CommandManager();
            inputField = GetComponentInChildren<TMP_InputField>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            SetVisability(visible);
            AddListeners();
            commandManager.RegisterObjectInstance(_settings);
        }

        private void AddListeners()
        {
            inputField.onSubmit.AddListener(inputField_OnSubmit);
            inputField.onValueChanged.AddListener(inputField_OnChanged);
            inputField.onDeselect.AddListener(HandleDeselect);
            inputField.onEndEdit.AddListener(HandleDeselect);
            // MethodHandler.OnSearchComplete += ShowAutocomplete;
        }

        private void HandleDeselect(string text)
        {
            RemoveTips();
        }

        void AddMessage(string message)
        {
            TMP_Text newMessage = Instantiate(messagePrefab);
            newMessage.SetText(message);
            newMessage.fontSize = _settings.GetTextSize();
            newMessage.transform.SetParent(windowContent, false);
            inputTexts.Add(newMessage);
        }

        private void inputField_OnChanged(string input)
        {
            if (input.Length == 0 || !IsCommand(input))
            {
                RemoveTips();
            }
            else
            {
                commandManager.FindCommandsStartingWithAsync(input.TrimStart(commandPrefix));
            }
            selectedLastCommand = lastInputs.Count;
            currentInput = input;

        }

        private void inputField_OnSubmit(string consoleInput)
        {
            if (string.IsNullOrEmpty(consoleInput))
            {
                return;
            }

            inputField.SetTextWithoutNotify(string.Empty);
            inputField.ActivateInputField();

            if (IsCommand(consoleInput))
            {
                string command = consoleInput.Trim(commandPrefix);


                if (commandManager.ExecuteMethod(command, out CommandExecutionResult result))
                {
                    if (result.ReturnedObject != null)
                    {
                        string stringResult = result.ReturnedObject.ToString();
                        AddMessage(stringResult);
                    }
                    
                }
                else
                {
                    AddMessage(result.ExecutionMessage);
                }
                //if (!string.IsNullOrEmpty(response))
                {
                    //  AddMessage(response);
                }

            }
            else
            {
                AddMessage(consoleInput);
            }
            RemoveTips();
            ResetInputHistoryScroller(consoleInput);

        }

        private void ResetInputHistoryScroller(string consoleInput)
        {
            lastInputs.Enqueue(consoleInput); // Store the input
            while (lastInputs.Count > maxStoredInputs)
            {
                lastInputs.Dequeue(); // Remove the oldest input if there are more than 5
            }

            currentInput = "";
            selectedLastCommand = lastInputs.Count;
            
        }

        private bool IsCommand(string consoleInput)
        {
            if (commandPrefix.Length == 0) return true;

            bool isCommand = false;

            foreach (var prefix in commandPrefix)
            {
                if (consoleInput.StartsWith(prefix))
                {
                    isCommand = true;
                }
            }

            return isCommand;
        }
        private void Update()
        {
            if (Input.GetKeyDown(toggleVisabilty))
            {
                SetVisability(!visible);
            }
            if(!visible)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow) && inputField.isFocused && lastInputs.Count > 0)
            {
                selectedLastCommand--;
                if (selectedLastCommand < 0)
                {
                    selectedLastCommand = lastInputs.Count - 1;
                    inputField.text = currentInput;
                }
                else
                {
                    inputField.SetTextWithoutNotify(lastInputs.ElementAt(selectedLastCommand));
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && inputField.isFocused && lastInputs.Count > 0)
            {
                selectedLastCommand++;
                if (selectedLastCommand >= lastInputs.Count)
                {
                    selectedLastCommand = 0;
                    inputField.text = currentInput;
                }
                else
                {
                    inputField.SetTextWithoutNotify(lastInputs.ElementAt(selectedLastCommand));
                }
            }
        }

        private void SetVisability(bool visible)
        {

            canvasGroup.alpha = visible ? visbleAlpha : 0;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            this.visible = visible;
        }

        private void ShowAutocomplete(List<MethodDescription> methodDescriptions)
        {
            RemoveTips();

            for (int i = 0; i < methodDescriptions.Count; i++)
            {
                MethodDescription method = methodDescriptions[i];
                TMP_Text newTip = Instantiate(messagePrefab);
                newTip.SetText(method.ToString());
                newTip.transform.SetParent(commandTipArea, false);
                if (i == 5) { break; }
            }
            int amountOfTime = methodDescriptions.Count > maxNumberOfTips ? maxNumberOfTips : methodDescriptions.Count;

            UpdateTipLayout(amountOfTime);
        }

        private void RemoveTips()
        {
            foreach (Transform tip in commandTipArea)
            {
                Destroy(tip.gameObject);

            }
            UpdateTipLayout(0);
        }

        private void UpdateTipLayout(int height)
        {

            var element = commandTipArea.GetComponent<LayoutElement>();
            element.preferredHeight = height * 100;
        }
        
        
    }
}
