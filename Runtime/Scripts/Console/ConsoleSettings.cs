using System;
using DragynGames.Commands;
using UnityEngine;

namespace DragynGames
{
    public class ConsoleSettings
    {
        SettingsData settingsData;

        public event Action OnSettingsChanged;


        public ConsoleSettings()
        {
            settingsData = new SettingsData();
            if (!TryLoadSettings())
            {
                settingsData = new SettingsData();
            }
        }


        #region Saving and loading settings

        void SaveSettings()
        {
            var jsonSettings = JsonUtility.ToJson(settingsData);
            string path = Application.persistentDataPath + "/settings.json";
            System.IO.File.WriteAllText(path, jsonSettings);
        }

        bool TryLoadSettings()
        {
            bool success = false;
            string path = Application.persistentDataPath + "/settings.json";
            if (System.IO.File.Exists(path))
            {
                string jsonSettings = System.IO.File.ReadAllText(path);
                settingsData = JsonUtility.FromJson<SettingsData>(jsonSettings);
                if (settingsData != null)
                {
                    success = true;
                }
            }

            return success;
        }

        #endregion

        #region TextSize

        [ConsoleAction("Setting.TextSize", "Sets the text size of the console", "textSize")]
        public void SetTextSize(int textSize)
        {
            settingsData.TextSize = textSize;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("Setting.TextSize", "Returns the size of the font")]
        public int GetTextSize() => settingsData.TextSize;

        #endregion

        #region Background

        [ConsoleAction("Setting.BGcolor",
            "Sets the background color of the console, ex. \"BGcolor red\" and \"BGcolor (1 0 0)\" both sets bg to red.",
            "color")]
        public void SetBackgroundColor(Color color)
        {
            settingsData.Color = color;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public Color GetBackgroundColor() => settingsData.Color;

        [ConsoleAction("Setting.Transparency", "Sets the transparency max 80 percentage", "transparency percentage")]
        public void SetTransparency(float transparencyPercentage)
        {
            float alpha;
            if (transparencyPercentage > 1)
            {
                transparencyPercentage = Mathf.Min(transparencyPercentage, 100);
                transparencyPercentage /= 100;
            }

            alpha = 1 - transparencyPercentage;
            settingsData.Alpha = alpha;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("Setting.Alpha", "Returns the transparency")]
        public float GetAlpha() => settingsData.Alpha;

        #endregion

        #region Scrolling

        public bool ShouldScrollToBottom => settingsData.ScrollToBottom;

        [ConsoleAction("Setting.FollowMessages", "Sets the console to scroll down to follow latest message", "Should follow")]
        public void SetScrollToBottom(bool value)
        {
            settingsData.ScrollToBottom = value;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        #endregion

        #region Logging

        public bool ShouldPrintLogs => settingsData.PrintLogs;

        [ConsoleAction("Setting.PrintLogs", "Sets the console to print logs", "Should print")]
        public void SetPrintLogs(bool value)
        {
            settingsData.PrintLogs = value;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("Setting.PrintStackTrace",
            "parameters: Error,Assert,Warning,Log,Exception. Ex [1 0 0 0 0] turns on errors stacktrace",
            "Should print")]
        public void SetAcceptedStackTraces(bool[] stackTraces)
        {
            settingsData.printStackTrace = stackTraces;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("Setting.PrintLogTypes",
            "parameters: Error,Assert,Warning,Log,Exception. Ex [1 0 0 0 0] turns only errors on", "Should print")]
        public void SetAcceptedLogTypes(bool[] logTypes)
        {
            settingsData.printLogTypes = logTypes;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public bool[] GetAcceptedLogTypes()
        {
            return settingsData.printLogTypes;
        }

        public bool[] GetAcceptedStackTraces()
        {
            return settingsData.printStackTrace;
        }

        #endregion

        [ConsoleAction("Setting.ResetSettings", "Resets the settings to default")]
        public void ResetSettings()
        {
            settingsData = new SettingsData();
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("Setting.GetSettings", "Returns the current settings")]
        public string GetSettings()
        {
            return JsonUtility.ToJson(settingsData, true);
        }

        public void SavePosition(Vector2 anchoredPosition, Vector2 rectSizeDelta)
        {
            settingsData.position = anchoredPosition;
            settingsData.size = rectSizeDelta;
            SaveSettings();
        }

        public Vector2 GetSize()
        {
            return settingsData.size;
        }

        public Vector2 GetPosition()
        {
            return settingsData.position;
        }
    }

    [Serializable]
    public class SettingsData
    {
        public int TextSize = 14;
        public Color Color = Color.white;
        public float Alpha = 0.7f;
        public bool ScrollToBottom = true;
        public bool PrintLogs = true;
        public bool[] printLogTypes = new bool[5] {true, true, true, true, true};
        public bool[] printStackTrace = new bool[5] {false, false, false, false, false};
        public Vector2 position;
        public Vector2 size;
        
    }
}