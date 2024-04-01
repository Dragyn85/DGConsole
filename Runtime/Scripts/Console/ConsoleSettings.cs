using System;
using System.Collections;
using System.Collections.Generic;
using DragynGames.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        [ConsoleAction("TextSize", "Sets the text size of the console", "textSize")]
        public void SetTextSize(int textSize)
        {
            settingsData.TextSize = textSize;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("TextSize", "Returns the size of the font")]
        public int GetTextSize() => settingsData.TextSize;

        #endregion

        #region Background

        [ConsoleAction("BGcolor",
            "Sets the background color of the console, ex. \"BGcolor red\" and \"BGcolor (1 0 0)\" both sets bg to red.",
            "color")]
        public void SetBackgroundColor(Color color)
        {
            settingsData.Color = color;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public Color GetBackgroundColor() => settingsData.Color;

        [ConsoleAction("Transparency", "Sets the transparency max 80 percentage", "transparency percentage")]
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

        [ConsoleAction("Alpha", "Returns the transparency")]
        public float GetAlpha() => settingsData.Alpha;

        #endregion

        #region Scrolling

        public bool ShouldScrollToBottom => settingsData.ScrollToBottom;

        [ConsoleAction("FollowMessages", "Sets the console to scroll down to follow latest message", "Should follow")]
        public void SetScrollToBottom(bool value)
        {
            settingsData.ScrollToBottom = value;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        #endregion

        #region Logging

        public bool ShouldPrintLogs => settingsData.PrintLogs;

        [ConsoleAction("PrintLogs", "Sets the console to print logs", "Should print")]
        public void SetPrintLogs(bool value)
        {
            settingsData.PrintLogs = value;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("PrintStackTrace",
            "parameters: Error,Assert,Warning,Log,Exception. Ex [1 0 0 0 0] turns on errors stacktrace",
            "Should print")]
        public void SetAcceptedStackTraces(bool[] stackTraces)
        {
            settingsData.printStackTrace = stackTraces;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("PrintLogTypes",
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

        [ConsoleAction("ResetSettings", "Resets the settings to default")]
        public void ResetSettings()
        {
            settingsData = new SettingsData();
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        [ConsoleAction("GetSettings", "Returns the current settings")]
        public string GetSettings()
        {
            return JsonUtility.ToJson(settingsData, true);
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
    }
}