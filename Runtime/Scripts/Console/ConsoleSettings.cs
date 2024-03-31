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
        //Image

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

        [ConsoleAction("BGcolor", "Sets the background color of the console", "color")]
        public void SetBackGroudColor(Color color)
        {
            settingsData.Color = color;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public Color GetBackgroundColor() => settingsData.Color;
        
        [ConsoleAction("Transparency", "Sets the transparency", "transparency percentage")]
        public void SetTransparency(float transparencyPercentage)
        {
            float alpha;
            if (transparencyPercentage > 1)
            {
                Mathf.Min(transparencyPercentage, 100);
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
        [ConsoleAction("ResetSettings", "Resets the settings to default")]
        public void ResetSettings()
        {
            settingsData = new SettingsData();
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }
    }

    [Serializable]
    public class SettingsData
    {
        public int TextSize = 14;
        public Color Color = Color.black;
        public float Alpha = 0.7f;
    }
}