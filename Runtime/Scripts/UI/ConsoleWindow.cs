using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragynGames.Console.UI
{
    internal class ConsoleWindow : MonoBehaviour
    {
        [Header("Message area")]
        [SerializeField] Transform windowContent;
        [SerializeField] TMP_Text messagePrefab;

        [Space(10)]
        [Header("Commands")]
        [SerializeField] char[] commandPrefix;
        [SerializeField] Transform commandTipArea;

        TMP_InputField inputField;
        [SerializeField] int maxNumberOfTips = 5;

        private void Awake()
        {

            inputField = GetComponentInChildren<TMP_InputField>();
            inputField.onSubmit.AddListener(inputField_OnSubmit);
            inputField.onValueChanged.AddListener(inputField_OnChanged);
            MethodHandler.OnSearchComplete += ShowAutocomplete;
        }

        void AddMessage(string message)
        {
            TMP_Text newMessage = Instantiate(messagePrefab);
            newMessage.SetText(message);
            newMessage.transform.SetParent(windowContent, false);
        }

        private void inputField_OnChanged(string arg0)
        {
            if (arg0.Length == 0 || !IsCommand(arg0)) return;

            MethodHandler.FindMethodsStartingAsync(arg0.TrimStart(commandPrefix));

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
                string response = MethodHandler.ExecuteMethod(consoleInput.TrimStart(commandPrefix));
                if (!string.IsNullOrEmpty(response))
                {
                    AddMessage(response);
                }

            }
            else
            {
                AddMessage(consoleInput);
            }
            RemoveTips();
            UpdateTipLayout(0);
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
            int amountOfTime = methodDescriptions.Count > maxNumberOfTips ? maxNumberOfTips:methodDescriptions.Count;

            UpdateTipLayout(amountOfTime);
        }

        private void RemoveTips()
        {
            foreach (Transform tip in commandTipArea)
            {
                Destroy(tip.gameObject);

            }
        }

        private void UpdateTipLayout(int height)
        {

            var element = commandTipArea.GetComponent<LayoutElement>();
            element.preferredHeight = height * 100;
        }
    }
}
