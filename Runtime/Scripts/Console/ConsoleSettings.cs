using System;
using System.Collections;
using System.Collections.Generic;
using DragynGames.Commands;
using TMPro;
using UnityEngine;

namespace DragynGames
{
    public class ConsoleSettings
    {
        SettingsData _settingsData;
        private List<TMP_Text> inputTexts;

        public ConsoleSettings(List<TMP_Text> messageList)
        {
            inputTexts = messageList;
            
            _settingsData = new SettingsData();
            if (!TryLoadSettings())
            {
                _settingsData = new SettingsData();
            }
        }

        void SaveSettings()
        {
            var jsonSettings = JsonUtility.ToJson(_settingsData);
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
                _settingsData = JsonUtility.FromJson<SettingsData>(jsonSettings);
                if (_settingsData != null)
                {
                    success = true;
                }
            }

            return success;
        }

        [ConsoleAction("SetTextSize", "Sets the text size of the console", "textSize")]
        void SetTextSize(int textSize)
        {
            _settingsData.textSize = textSize;
            SaveSettings();
            foreach (var VARIABLE in inputTexts)
            {
                VARIABLE.fontSize = textSize;
            }
        }
        
        public int GetTextSize()
        {
            return _settingsData.textSize;
        }
    }

    [Serializable]
    public class SettingsData
    {
        public int textSize;
    }
}