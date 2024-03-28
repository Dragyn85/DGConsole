using DragynGames.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DragynGames
{
    public static class LogHelpMethods
    {

        // Returns the list of available commands
        internal static string LogAllCommands(List<CommandInfo> methods)
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

            return stringBuilder.ToString();
        }

        // Logs the list of available commands that are either equal to commandName or contain commandName as substring
        internal static string LogAllCommandsWithName(string commandName, Func<string,bool,List<CommandInfo>,CompareInfo,List<CommandInfo>> findCommands, List<CommandInfo> searchedMethods)
        {
            List<CommandInfo> matchingMethods = new List<CommandInfo>();
            CompareInfo compareInfo = CommandManager._settings.caseInsensitiveComparer;
            // First, try to find commands that exactly match the commandName. If there are no such commands, try to find
            // commands that contain commandName as substring
            matchingMethods = findCommands(commandName, false,searchedMethods,compareInfo);
            if (matchingMethods.Count == 0)
                matchingMethods = findCommands(commandName, false,searchedMethods,compareInfo);

            if (matchingMethods.Count == 0)
            {
                return $"No commands matching {commandName} found";
            }
            else
            {
                int commandsLength = 25;
                for (int i = 0; i < matchingMethods.Count; i++)
                    commandsLength += matchingMethods[i].signature.Length + 7;

                StringBuilder stringBuilder = new StringBuilder(commandsLength);
                stringBuilder.Append("Matching commands:");

                for (int i = 0; i < matchingMethods.Count; i++)
                    stringBuilder.Append("\n    - ").Append(matchingMethods[i].signature);

                return stringBuilder.ToString();
            }
        }
    }

}
