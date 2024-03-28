using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Commands
{
    public class ConsoleBuiltInActions
    {
        // Logs the list of available commands
        [ConsoleAction("LogAllCommand", "Logs and returns all available commands")]
        public static string LogAllCommands()
        {
            List<CommandInfo> methods = CommandManager.GetMethods();
            string allCommands = LogHelpMethods.LogAllCommands(methods);
            Debug.Log(allCommands);
            return allCommands;
        }

        // Logs the list of available commands that are either equal to commandName or contain commandName as substring
        internal static void LogAllCommandsWithName(string commandName, int maxResults = 4)
        {
            List<CommandInfo> matchingMethods = new List<CommandInfo>(maxResults);
            string AllMatchingMethods =
                LogHelpMethods.LogAllCommandsWithName(commandName, CachedMethodFinder.FindCommands, matchingMethods);

            Debug.Log(AllMatchingMethods);
        }

        // Logs system information
        public static void LogSystemInfo()
        {
            Debug.Log(SystemInfoLogger.LogSystemInfo());
        }
    }
}