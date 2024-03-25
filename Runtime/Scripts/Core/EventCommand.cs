using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace DragynGames.Console
{
    public class EventCommand : MonoBehaviour
    {
        [SerializeField] string commandName;
        [SerializeField] UnityEvent OnCommand;
        
        static Dictionary<string,EventCommand> commands = new Dictionary<string, EventCommand>();

        private void Awake()
        {
            if(string.IsNullOrEmpty(commandName))
            {
                Debug.LogError("Command name is empty");
                return;
            }
            if(commands.ContainsKey(commandName))
            {
                Debug.LogError($"Command {commandName} already exists");
                return;
            }
            
            commands.Add(commandName.ToLower(), this);
        }

        [ConsoleAction("cmd", "Invokes the command", "commandname")]
        static void InvokeCommand(string commandname)
        {
            if(commands.TryGetValue(commandname.ToLower(), out EventCommand command))
            {
                command.OnCommand.Invoke();
            }
            else
            {
                Debug.LogError($"Command {commandname} not found");
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Create a pattern that matches any character that is not a letter, number, delimiter, or whitespace
            string pattern = "[^a-zA-Z]";

            // Replace invalid characters with an empty string
            commandName = Regex.Replace(commandName, pattern, "");
            
        }
#endif
    }
}
