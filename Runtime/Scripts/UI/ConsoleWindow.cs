using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
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
        private bool visible;
        private CanvasGroup canvasGroup;

        [SerializeField] Assembly[] assembly;
        CommandManager commandManager;
        private void Awake()
        {
            commandManager = new CommandManager();
            inputField = GetComponentInChildren<TMP_InputField>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            AddListeners();
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
            newMessage.transform.SetParent(windowContent, false);
        }

        private void inputField_OnChanged(string arg0)
        {
            if (arg0.Length == 0 || !IsCommand(arg0))
            {
                RemoveTips();
                return;
            }

            CommandManager.FindCommandsStartingWithAsync(arg0.TrimStart(commandPrefix));

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
