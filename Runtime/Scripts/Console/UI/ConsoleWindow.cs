using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DragynGames.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using DragynGames.Console.UI;
using UnityEngine.Serialization;

namespace DragynGames.Console
{
    public class ConsoleWindow : MonoBehaviour
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


        [Space(10)] [Header("Commands")] [SerializeField]
        char[] commandPrefix;

        [SerializeField] Transform commandTipArea;
        [SerializeField] int maxNumberOfTips = 5;
        [SerializeField] bool visible;
        [SerializeField] TMP_Text targetDisplay;
        
        public event Action<bool> OnVisibilityChanged;
        
        Assembly[] assembly;
        TMP_InputField inputField;
        CanvasGroup canvasGroup;
        CommandManager commandManager;
        List<TMP_Text> messageTexts = new();

        private Queue<ConsoleCommand> lastInputs = new();
        private int selectedLastCommand;
        private int maxStoredInputs = 10;
        private ConsoleCommand currentInput;

        public NotifyingValue<GameObject> CurrentTarget;


        int selectedSuggestionIndex = 0;
        List<string> currentSuggestions = new List<string>();

        private ConsoleSettings _settings;
        List<IConsoleComponent> consoleComponents = new();

        private ConsoleEntryHistoryTracker _consoleEntryHistoryTracker = new();

        [FormerlySerializedAs("highlighMaterial")] [FormerlySerializedAs("outlineMaterial")] [SerializeField]
        Material highlightMaterial;

        private int currentCaretPosition;

        private void Awake()
        {
            CurrentTarget =
                new NotifyingValue<GameObject>(null,
                    (target) => { targetDisplay.SetText(target != null ? target.name : ""); });
            commandManager = new CommandManager();
            _settings = new ConsoleSettings();
            commandManager.RegisterObjectInstance(_settings);
            commandManager.RegisterObjectInstance(this);

            foreach (var consoleComponent in GetComponents<IConsoleComponent>())
            {
                AttachConsoleComponent(consoleComponent);
            }

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

        private void AttachConsoleComponent(IConsoleComponent consoleComponent)
        {
            consoleComponent.OnConsoleWindowAttached(this);
            consoleComponents.Add(consoleComponent);
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
        public void ActivateInputMode()
        {
            inputField.ActivateInputField();
            inputField.caretPosition = currentCaretPosition;
            inputField.selectionAnchorPosition = currentCaretPosition;
            inputField.selectionFocusPosition = currentCaretPosition;
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

            if (isAcceptedType && shouldPrintStacktrace)
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
            _settings.SavePosition(window.anchoredPosition, window.sizeDelta);
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
            currentCaretPosition = inputField.caretPosition;
            commandManager.FindCommandsStartingWithAsync(input.TrimStart(commandPrefix));
            selectedLastCommand = lastInputs.Count;
            currentInput = new ConsoleCommand(input, CurrentTarget.Get());
            commandManager.GetSuggestions(input.TrimStart(commandPrefix), ShowAutocomplete);
            
        }

        private void inputField_OnSubmit(string consoleInput)
        {
            SendCommand(consoleInput, CurrentTarget.Get());
            CurrentTarget.Set(null); // Clear the cached target after sending the command
        }

        private void SendCommand(string consoleInput, GameObject instance = null)
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

                if (commandManager.ExecuteMethod(command, out CommandExecutionResult result, instance))
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
            }
            else
            {
                AddMessage(consoleInput);
            }

            RemoveTips();
            _consoleEntryHistoryTracker.AddEntry(new ConsoleCommand(consoleInput, instance));
            _consoleEntryHistoryTracker.ResetHistorySelection();
            currentInput = new ConsoleCommand();
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
                CurrentTarget.Set(null);
            }

            if (!visible)
                return;

            foreach (var consoleComponent in consoleComponents)
            {
                consoleComponent.Tick(Time.deltaTime);
            }


            HandleHistorySelection();
            HandleSuggestionSelection();
        }

        private void HandleSuggestionSelection()
        {
            if ((Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftControl)) &&
                currentSuggestions.Count > 0)
            {
                selectedSuggestionIndex--;
                if (selectedSuggestionIndex < 0)
                {
                    selectedSuggestionIndex = currentSuggestions.Count - 1;
                }

                string prefix = commandPrefix.Length > 0 ? commandPrefix[0].ToString() : "";
                inputField.SetTextWithoutNotify($"{prefix}{currentSuggestions[selectedSuggestionIndex]}");
                inputField.caretPosition = inputField.text.Length;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftControl) &&
                currentSuggestions.Count > 0)
            {
                selectedSuggestionIndex++;
                if (selectedSuggestionIndex >= currentSuggestions.Count)
                {
                    selectedSuggestionIndex = 0;
                }

                string prefix = commandPrefix.Length > 0 ? commandPrefix[0].ToString() : "";
                inputField.SetTextWithoutNotify($"{prefix}{currentSuggestions[selectedSuggestionIndex]}");
                inputField.caretPosition = inputField.text.Length;
            }
        }

        private void HandleHistorySelection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftControl) && inputField.isFocused &&
                _consoleEntryHistoryTracker.HasEntries())
            {
                bool success = _consoleEntryHistoryTracker.GetNextEntry(out ConsoleCommand oldCommand);
                if (!success)
                {
                    inputField.text = currentInput.Command;
                    CurrentTarget.Set(currentInput.Target);
                }
                else
                {
                    SetInputFieldTextUnnotified(oldCommand);
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) &&
                !Input.GetKey(KeyCode.LeftControl) &&
                inputField.isFocused &&
                _consoleEntryHistoryTracker.HasEntries())
            {
                bool success = _consoleEntryHistoryTracker.GetPreviousEntry(out ConsoleCommand oldCommand);
                if (!success)
                {
                    inputField.text = currentInput.Command;
                    CurrentTarget.Set(currentInput.Target);
                }
                else
                {
                    SetInputFieldTextUnnotified(oldCommand);
                }
            }
        }

        private void SetInputFieldTextUnnotified(ConsoleCommand oldCommand)
        {
            inputField.SetTextWithoutNotify(oldCommand.Command);
            inputField.caretPosition = inputField.text.Length;
            CurrentTarget.Set(oldCommand.Target);
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
            
            OnVisibilityChanged?.Invoke(visible);
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

    public struct ConsoleCommand
    {
        public string Command;
        public GameObject Target;

        public ConsoleCommand(string command, GameObject target)
        {
            Command = command;
            Target = target;
        }
    }

    public interface IConsoleComponent
    {
        public void Tick(float deltaTime);
        public void OnConsoleWindowAttached(ConsoleWindow consoleWindow);
    }
}