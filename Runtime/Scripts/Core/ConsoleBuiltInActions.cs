using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Console
{
    public class ConsoleBuiltInActions
    {
        public static void AddBuiltInCommands()
        {
            MethodHandler.AddCommand("help", "Prints all commands", LogAllCommands, true);
            MethodHandler.AddCommand<string, int>("help", "Prints all matching commands", LogAllCommandsWithName, true);
            MethodHandler.AddCommand("sysinfo", "Prints system information", LogSystemInfo, true);
        }

        // Logs the list of available commands
        public static void LogAllCommands()
        {
            List<ConsoleMethodInfo> methods = MethodHandler.GetMethods();
            string allCommands = LogHelpMethods.LogAllCommands(methods);
            Debug.Log(allCommands);
        }

        // Logs the list of available commands that are either equal to commandName or contain commandName as substring
        public static void LogAllCommandsWithName(string commandName, int maxResults = 4)
        {
            List<ConsoleMethodInfo> matchingMethods = new List<ConsoleMethodInfo>(maxResults);
            string AllMatchingMethods =
                LogHelpMethods.LogAllCommandsWithName(commandName, MethodHandler.FindCommands, matchingMethods);

            Debug.Log(AllMatchingMethods);
        }

        // Logs system information
        public static void LogSystemInfo()
        {
            Debug.Log(SystemInfoLogger.LogSystemInfo());
        }
    }
}