using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragynGames.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using DragynGames.Console.UI;

namespace DragynGames.Console
{
    internal class ConsoleWindow : MonoBehaviour
    {
        [Header("Visibility")] [SerializeField]
        KeyCode toggleVisabilty = KeyCode.L;

        [SerializeField, Range(0, 1)] float visbleAlpha = 1;


        [Space(10)] [Header("Message area")] [SerializeField]
        Transform windowContent;

        [SerializeField] TMP_Text messagePrefab;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Image backGround;
        [SerializeField] RectTransform window;


        [Space(10)] [Header("Commands")] 
        [SerializeField] char[] commandPrefix;

        [SerializeField] Transform commandTipArea;

        TMP_InputField inputField;
        [SerializeField] int maxNumberOfTips = 5;
        [SerializeField] bool visible;
        private CanvasGroup canvasGroup;

        [SerializeField] Assembly[] assembly;
        CommandManager commandManager;
        List<TMP_Text> messageTexts = new();


        private Queue<string> lastInputs = new();
        private int selectedLastCommand;
        private int maxStoredInputs = 10;
        private string currentInput;
        
        int selectedSuggestionIndex = 0;
        List<string> currentSuggestions = new List<string>();

        private ConsoleSettings _settings;

        private void Awake()
        {
            commandManager = new CommandManager();
            _settings = new ConsoleSettings();
            commandManager.RegisterObjectInstance(_settings);
            commandManager.RegisterObjectInstance(this);

            inputField = GetComponentInChildren<TMP_InputField>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            SetVisability(visible);
            AddListeners();

            UpdateSettings();
            
            var size = _settings.GetSize();
            var position = _settings.GetPosition();
            if (size != Vector2.zero)
            {
                window.sizeDelta = size;
                window.anchoredPosition = position;
            }
            
        }

        private void OnDestroy()
        {
            _settings.OnSettingsChanged -= UpdateSettings;
            Application.logMessageReceived -= AddLogMessage;
        }
        
        [ConsoleAction("Clean", "Removes all messages from the console")]
        private void RemoveMessages()
        {
            foreach (var message in messageTexts)
            {
                Destroy(message.gameObject);
            }
            messageTexts.Clear();
        }

        private void UpdateSettings()
        {
            foreach (var text in messageTexts)
            {
                text.fontSize = _settings.GetTextSize();
            }

            backGround.color = _settings.GetBackgroundColor();
            visbleAlpha = _settings.GetAlpha();
            SetVisability(visible);

            if (_settings.ShouldPrintLogs)
            {
                Application.logMessageReceived -= AddLogMessage;
                Application.logMessageReceived += AddLogMessage;
            }
            else
            {
                Application.logMessageReceived -= AddLogMessage;
            }
        }

        private void AddLogMessage(string condition, string stacktrace, LogType type)
        {
            if (!Application.isPlaying)
                return;
            
            bool isAcceptedType = _settings.GetAcceptedLogTypes()[(int) type];
            bool shouldPrintStacktrace = _settings.GetAcceptedStackTraces()[(int) type];
            
            if (isAcceptedType)
            {
                PrintLogText(condition, type);
            }
            if(isAcceptedType && shouldPrintStacktrace)
            {
                PrintLogText(stacktrace, type);
            }
        }

        private void PrintLogText(string text, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    AddMessage($"<color=red>{text}</color>");
                    break;
                case LogType.Warning:
                    AddMessage($"<color=yellow>{text}</color>");
                    break;
                case LogType.Log:
                    AddMessage(text);
                    break;
            }
        }

        private void AddListeners()
        {
            inputField.onSubmit.AddListener(inputField_OnSubmit);
            inputField.onValueChanged.AddListener(inputField_OnChanged);
            inputField.onDeselect.AddListener(HandleDeselect);
            inputField.onEndEdit.AddListener(HandleDeselect);
            _settings.OnSettingsChanged += UpdateSettings;
            GetComponentInChildren<Move>().OnMoveFinished += HandlePositionChange;
            GetComponentInChildren<Resize>().OnResizeFinished += HandlePositionChange;
        }
        
        private void HandlePositionChange()
        {
            _settings.SavePosition(window.anchoredPosition,window.sizeDelta);
        }

        private void HandleDeselect(string text)
        {
            RemoveTips();
        }

        void AddMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            
            TMP_Text newMessage = Instantiate(messagePrefab);
            newMessage.SetText(message);
            newMessage.fontSize = _settings.GetTextSize();
            newMessage.transform.SetParent(windowContent, false);
            messageTexts.Add(newMessage);

            if (_settings.ShouldScrollToBottom)
                StartCoroutine(ScrollToBottom());
        }

        IEnumerator ScrollToBottom()
        {
            yield return null;
            scrollRect.verticalNormalizedPosition = 0;
        }

        private void inputField_OnChanged(string input)
        {
            if (input.Length == 0 || !IsCommand(input))
            {
                RemoveTips();
                return;
            }
            else
            {
                commandManager.FindCommandsStartingWithAsync(input.TrimStart(commandPrefix));
            }

            selectedLastCommand = lastInputs.Count;
            currentInput = input;
            commandManager.GetSuggestions(input.TrimStart(commandPrefix), ShowAutocomplete);
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

        private bool IsCommand(string text)
        {
            if (commandPrefix.Length == 0) return true;

            bool isCommand = false;

            foreach (var prefix in commandPrefix)
            {
                if (text.StartsWith(prefix))
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

            if (!visible)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftControl) && inputField.isFocused && lastInputs.Count > 0)
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
                    inputField.caretPosition = inputField.text.Length;
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftControl) && inputField.isFocused && lastInputs.Count > 0)
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
                    inputField.caretPosition = inputField.text.Length;
                }
            }
            if ((Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftControl)) && currentSuggestions.Count > 0)
            {
                selectedSuggestionIndex--;
                if (selectedSuggestionIndex < 0)
                {
                    selectedSuggestionIndex = currentSuggestions.Count - 1;
                }

                string prefix = commandPrefix.Length > 0 ? commandPrefix[0].ToString() : "" ;
                inputField.SetTextWithoutNotify($"{prefix}{currentSuggestions[selectedSuggestionIndex]}");
                inputField.caretPosition = inputField.text.Length;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) &&Input.GetKey(KeyCode.LeftControl) && currentSuggestions.Count > 0)
            {
                selectedSuggestionIndex++;
                if (selectedSuggestionIndex >= currentSuggestions.Count)
                {
                    selectedSuggestionIndex = 0;
                }
                string prefix = commandPrefix.Length > 0 ? commandPrefix[0].ToString() : "" ;
                inputField.SetTextWithoutNotify($"{prefix}{currentSuggestions[selectedSuggestionIndex]}");
                inputField.caretPosition = inputField.text.Length;
            }
        }

        private void SetVisability(bool visible)
        {
            canvasGroup.alpha = visible ? Mathf.Max(visbleAlpha, 0.2f) : 0;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            this.visible = visible;

            if (visible)
            {
                inputField.ActivateInputField();
                inputField.SetTextWithoutNotify("");
            }
        }

        private void ShowAutocomplete(List<SuggestionData> suggestionDatas)
        {
            RemoveTips();
            List<string> suggestions = suggestionDatas.Select(t => t.command).ToList();
            currentSuggestions = suggestions;
            selectedSuggestionIndex = -1;

            for (int i = 0; i < suggestionDatas.Count; i++)
            {
                string method = suggestionDatas[i].suggestionText;
                TMP_Text newTip = Instantiate(messagePrefab);
                newTip.SetText(method);
                newTip.transform.SetParent(commandTipArea, false);
                if (i == 5)
                {
                    break;
                }
            }

            int amountOfTime = suggestionDatas.Count > maxNumberOfTips ? maxNumberOfTips : suggestionDatas.Count;

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