using DragynGames.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DragynGames
{
    public static class LogHelpMethods
    {

        // Logs the list of available commands
        public static void LogAllCommands()
        {
            int length = 25;
            for (int i = 0; i < methods.Count; i++)
            {
                if (methods[i].IsValid())
                    length += methods[i].signature.Length + 7;
            }

            StringBuilder stringBuilder = new StringBuilder(length);
            stringBuilder.Append("Available commands:");

            for (int i = 0; i < methods.Count; i++)
            {
                if (methods[i].IsValid())
                    stringBuilder.Append("\n    - ").Append(methods[i].signature);
            }

            Debug.Log(stringBuilder.ToString());

            // After typing help, the log that lists all the commands should automatically be expanded for better UX
            LongMessageAdded?.Invoke();
            //if (DebugLogManager.Instance)
            //.Instance.AdjustLatestPendingLog(true, true);
        }

        // Logs the list of available commands that are either equal to commandName or contain commandName as substring
        public static void LogAllCommandsWithName(string commandName, Action<string,bool,List<ConsoleMethodInfo>> FindCommands, List<ConsoleMethodInfo> matchingMethods)
        {
            matchingMethods.Clear();

            // First, try to find commands that exactly match the commandName. If there are no such commands, try to find
            // commands that contain commandName as substring
            FindCommands(commandName, false, matchingMethods);
            if (matchingMethods.Count == 0)
                FindCommands(commandName, true, matchingMethods);

            if (matchingMethods.Count == 0)
                Debug.LogWarning(string.Concat("ERROR: can't find command '", commandName, "'"));
            else
            {
                int commandsLength = 25;
                for (int i = 0; i < matchingMethods.Count; i++)
                    commandsLength += matchingMethods[i].signature.Length + 7;

                StringBuilder stringBuilder = new StringBuilder(commandsLength);
                stringBuilder.Append("Matching commands:");

                for (int i = 0; i < matchingMethods.Count; i++)
                    stringBuilder.Append("\n    - ").Append(matchingMethods[i].signature);

                Debug.Log(stringBuilder.ToString());


                LongMessageAdded?.Invoke();
                //if (DebugLogManager.Instance)
                //  DebugLogManager.Instance.AdjustLatestPendingLog(true, true);
            }
        }
    }

}
