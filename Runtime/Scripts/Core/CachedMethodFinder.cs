using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace DragynGames.Commands
{
    public class CachedMethodFinder
    {
        // Finds all commands that have a matching signature with command
        // - caretIndexIncrements: indices inside "string command" that separate two arguments in the command. This is used to
        //   figure out which argument the caret is standing on
        // - commandName: command's name (first argument)
        internal void GetCommandSuggestions(string command, IReadOnlyList<CommandInfo> methods,
            List<CommandInfo> matchingCommands,
            List<int> caretIndexIncrements, CompareInfo caseInsensitiveComparer, ref string commandName,
            out int numberOfParameters)
        {
            bool commandNameCalculated = false;
            bool commandNameFullyTyped = false;
            numberOfParameters = -1;

            for (int i = 0; i < command.Length; i++)
            {
                if (char.IsWhiteSpace(command[i]))
                    continue;

                int delimiterIndex = CommandTypeParser.IndexOfDelimiterGroup(command[i]);
                if (delimiterIndex >= 0)
                {
                    i = HandleDelimiterGroup(command, ref commandName, ref commandNameCalculated,
                        ref commandNameFullyTyped, i, delimiterIndex, caretIndexIncrements, caseInsensitiveComparer);
                }
                else
                {
                    i = HandleNonDelimiterGroup(command, ref commandName, ref commandNameCalculated,
                        ref commandNameFullyTyped, i, caretIndexIncrements, caseInsensitiveComparer);
                }

                numberOfParameters++;
            }

            if (!commandNameCalculated)
                commandName = string.Empty;

            if (!string.IsNullOrEmpty(commandName))
            {
                HandleCommandName(commandName, ref commandNameFullyTyped, numberOfParameters, matchingCommands, methods,
                    caseInsensitiveComparer);
            }
        }

        private int HandleDelimiterGroup(string command, ref string commandName, ref bool commandNameCalculated,
            ref bool commandNameFullyTyped, int i, int delimiterIndex, List<int> caretIndexIncrements,
            CompareInfo caseInsensitiveComparer)
        {
            int endIndex = CommandTypeParser.IndexOfDelimiterGroupEnd(command, delimiterIndex, i + 1);
            if (!commandNameCalculated)
            {
                commandNameCalculated = true;
                commandNameFullyTyped = command.Length > endIndex;

                int commandNameLength = endIndex - i - 1;
                if (commandName == null || commandNameLength == 0 || commandName.Length != commandNameLength ||
                    caseInsensitiveComparer.IndexOf(command, commandName, i + 1, commandNameLength,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) != i + 1)
                    commandName = command.Substring(i + 1, commandNameLength);
            }

            i = (endIndex < command.Length - 1 && command[endIndex + 1] == ',') ? endIndex + 1 : endIndex;
            caretIndexIncrements.Add(i + 1);
            return i;
        }

        private int HandleNonDelimiterGroup(string command, ref string commandName, ref bool commandNameCalculated,
            ref bool commandNameFullyTyped, int i, List<int> caretIndexIncrements, CompareInfo caseInsensitiveComparer)
        {
            int endIndex = CommandTypeParser.IndexOfChar(command, ' ', i + 1);
            if (!commandNameCalculated)
            {
                commandNameCalculated = true;
                commandNameFullyTyped = command.Length > endIndex;

                int commandNameLength = command[endIndex - 1] == ',' ? endIndex - 1 - i : endIndex - i;
                if (commandName == null || commandNameLength == 0 || commandName.Length != commandNameLength ||
                    caseInsensitiveComparer.IndexOf(command, commandName, i, commandNameLength,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) != i)
                    commandName = command.Substring(i, commandNameLength);
            }

            i = endIndex;
            caretIndexIncrements.Add(i);
            return i;
        }

        private void HandleCommandName(string commandName, ref bool commandNameFullyTyped, int numberOfParameters,
            List<CommandInfo> matchingCommands, IReadOnlyList<CommandInfo> methods,
            CompareInfo caseInsensitiveComparer)
        {
            int commandIndex = FindCommandIndex(commandName, methods, caseInsensitiveComparer);
            if (commandIndex < 0)
                commandIndex = ~commandIndex;

            int commandLastIndex = commandIndex;
            if (!commandNameFullyTyped)
            {
                commandLastIndex = MatchAllCommandsStartingWith(commandName, commandIndex, commandLastIndex, methods,
                    caseInsensitiveComparer);
            }
            else
            {
                commandLastIndex = MatchAllCommandsEqualTo(commandName, commandIndex, commandLastIndex, methods,
                    caseInsensitiveComparer);
            }

            for (; commandIndex <= commandLastIndex; commandIndex++)
            {
                if (methods[commandIndex].parameterTypes.Length >= numberOfParameters)
                    matchingCommands.Add(methods[commandIndex]);
            }
        }

        internal static int FindCommandIndex(string command, IReadOnlyList<CommandInfo> methods,
            CompareInfo caseInsensitiveComparer)
        {
            int min = 0;
            int max = methods.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                int comparison = caseInsensitiveComparer.Compare(command, methods[mid].command,
                    CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
                if (comparison == 0)
                    return mid;
                else if (comparison < 0)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return ~min;
        }

        private int MatchAllCommandsStartingWith(string commandName, int commandIndex, int commandLastIndex,
            IReadOnlyList<CommandInfo> methods, CompareInfo caseInsensitiveComparer)
        {
            if (commandIndex < methods.Count && caseInsensitiveComparer.IsPrefix(methods[commandIndex].command,
                    commandName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
            {
                while (commandIndex > 0 && caseInsensitiveComparer.IsPrefix(methods[commandIndex - 1].command,
                           commandName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
                    commandIndex--;
                while (commandLastIndex < methods.Count - 1 && caseInsensitiveComparer.IsPrefix(
                           methods[commandLastIndex + 1].command, commandName,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
                    commandLastIndex++;
            }
            else
                commandLastIndex = -1;

            return commandLastIndex;
        }

        private int MatchAllCommandsEqualTo(string commandName, int commandIndex, int commandLastIndex,
            IReadOnlyList<CommandInfo> methods, CompareInfo caseInsensitiveComparer)
        {
            if (commandIndex < methods.Count && caseInsensitiveComparer.Compare(methods[commandIndex].command,
                    commandName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
            {
                while (commandIndex > 0 && caseInsensitiveComparer.Compare(methods[commandIndex - 1].command,
                           commandName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandIndex--;
                while (commandLastIndex < methods.Count - 1 &&
                       caseInsensitiveComparer.Compare(methods[commandLastIndex + 1].command, commandName,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandLastIndex++;
            }
            else
                commandLastIndex = -1;

            return commandLastIndex;
        }

        internal static List<CommandInfo> GetMatchingMethods(List<string> commandArguments,
            List<CommandInfo> methods, CompareInfo caseInsensitiveComparer)
        {
            List<CommandInfo> matchingMethods = new List<CommandInfo>();
            bool parameterCountMismatch = false;
            int commandIndex = FindCommandIndex(commandArguments[0], methods, caseInsensitiveComparer);

            if (commandIndex >= 0)
            {
                string _command = commandArguments[0];
                int commandLastIndex = commandIndex;

                while (commandIndex > 0 && caseInsensitiveComparer.Compare(methods[commandIndex - 1].command, _command,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandIndex--;

                while (commandLastIndex < methods.Count - 1 && caseInsensitiveComparer.Compare(
                           methods[commandLastIndex + 1].command, _command,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandLastIndex++;

                while (commandIndex <= commandLastIndex)
                {
                    if (!methods[commandIndex].IsValid())
                    {
                        methods.RemoveAt(commandIndex);
                        commandLastIndex--;
                    }
                    else
                    {
                        // Check if number of parameters match
                        if (methods[commandIndex].parameterTypes.Length == commandArguments.Count - 1)
                            matchingMethods.Add(methods[commandIndex]);
                        else
                            parameterCountMismatch = true;

                        commandIndex++;
                    }
                }
            }

            if (matchingMethods.Count == 0)
            {
                string _command = commandArguments[0];
                matchingMethods = FindCommands(_command, !parameterCountMismatch, methods, caseInsensitiveComparer);

                if (matchingMethods.Count == 0)
                    Debug.LogWarning(string.Concat("ERROR: can't find command '", _command, "'"));
                else
                {
                    int commandsLength = _command.Length + 75;
                    for (int i = 0; i < matchingMethods.Count; i++)
                        commandsLength += matchingMethods[i].signature.Length + 7;

                    StringBuilder stringBuilder = new StringBuilder(commandsLength);
                    if (parameterCountMismatch)
                        stringBuilder.Append("ERROR: '").Append(_command).Append("' doesn't take ")
                            .Append(commandArguments.Count - 1).Append(" parameter(s). Available command(s):");
                    else
                        stringBuilder.Append("ERROR: can't find command '").Append(_command).Append("'. Did you mean:");

                    for (int i = 0; i < matchingMethods.Count; i++)
                        stringBuilder.Append("\n    - ").Append(matchingMethods[i].signature);

                    Debug.LogWarning(stringBuilder.ToString());
                }
            }


            return matchingMethods;
        }

        internal static List<CommandInfo> FindCommands(string commandName, bool allowSubstringMatching,
            List<CommandInfo> methods, CompareInfo caseInsensitiveComparer)
        {
            List<CommandInfo> matchingCommands = new List<CommandInfo>();
            
            if (allowSubstringMatching)
            {
                for (int i = 0; i < methods.Count; i++)
                {
                    if (methods[i].IsValid() && caseInsensitiveComparer.IndexOf(methods[i].command, commandName,
                            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                        matchingCommands.Add(methods[i]);
                }
            }
            else
            {
                for (int i = 0; i < methods.Count; i++)
                {
                    if (methods[i].IsValid() && caseInsensitiveComparer.Compare(methods[i].command, commandName,
                            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                        matchingCommands.Add(methods[i]);
                }
            }

            return matchingCommands;
        }
    }
}