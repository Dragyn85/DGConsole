#if UNITY_EDITOR || UNITY_STANDALONE
// Unity's Text component doesn't render <b> tag correctly on mobile devices
#define USE_BOLD_COMMAND_SIGNATURES
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using SystemInfo = UnityEngine.Device.SystemInfo; // To support Device Simulator on Unity 2021.1+
#endif

// Manages the console commands, parses console input and handles execution of commands
// Supported method parameter types: int, float, bool, string, Vector2, Vector3, Vector4

// Helper class to store important information about a command
namespace DragynGames.Console
{
    public partial class MethodHandler
    {
        public static event Action LongMessageAdded;
        private const string MONOCOMMANDINDICATOR = ">";

        public delegate bool ParseFunction(string input, out object output);

        // All the commands
        private static readonly List<ConsoleMethodInfo> methods = new List<ConsoleMethodInfo>();
        private static readonly List<ConsoleMethodInfo> matchingMethods = new List<ConsoleMethodInfo>(4);

        // Split arguments of an entered command
        private readonly List<string> commandArguments = new List<string>(8);


        // CompareInfo used for case-insensitive command name comparison
        internal static readonly CompareInfo caseInsensitiveComparer = new CultureInfo("en-US").CompareInfo;

        private static CommandTypeParser _commandTypeParser;

        public static void RegisterObjectInstance(object consoleAction)
        {
            var instance = consoleAction;
            Type type = instance.GetType();

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance |
                                                          BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.DeclaredOnly))
            {
                foreach (ConsoleActionAttribute consoleMethod in method.GetCustomAttributes(
                             typeof(ConsoleActionAttribute),
                             false))
                {
                    if (consoleMethod != null)
                    {
                        var command = methods.FirstOrDefault(t => AreMethodsEqual(t.method, method));
                        if (command != null)
                        {
                            command.SetInstance(instance);
                        }
                    }
                }
            }
        }

        public static bool AreMethodsEqual(MethodInfo method1, MethodInfo method2)
        {
            if (method1.Name != method2.Name)
                return false;

            if (method1.DeclaringType != method2.DeclaringType)
                return false;

            if (method1.ReturnType != method2.ReturnType)
                return false;

            var parameters1 = method1.GetParameters();
            var parameters2 = method2.GetParameters();

            if (parameters1.Length != parameters2.Length)
                return false;

            for (int i = 0; i < parameters1.Length; i++)
            {
                if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                    return false;
            }

            if (method1.IsGenericMethod != method2.IsGenericMethod)
                return false;

            if (method1.IsGenericMethod)
            {
                var genericArguments1 = method1.GetGenericArguments();
                var genericArguments2 = method2.GetGenericArguments();

                if (genericArguments1.Length != genericArguments2.Length)
                    return false;

                for (int i = 0; i < genericArguments1.Length; i++)
                {
                    if (genericArguments1[i] != genericArguments2[i])
                        return false;
                }
            }

            return true;
        }

        public MethodHandler()
        {
            ConsoleBuiltInActions.AddBuiltInCommands();
            CommandMethodAssemblyFinder cmaf = new();
            var commandDefinitions = cmaf.SearchForCommands();
            foreach (var commandDefinition in commandDefinitions)
            {
            }
        }


        // Add a custom Type to the list of recognized command parameter Types


        // Remove a custom Type from the list of recognized command parameter Types
        public void RemoveCustomParameterType(Type type)
        {
            CommandTypeParser.RemoveType(type);
            ReadableTypes.Remove(type);
        }

        private static void AddCommand(string command, string description, MethodInfo method, bool instanced,
            object instance,
            string[] parameterNames)
        {
            var consoleMethodInfo = CreateConsoleMethodInfo(command, description, method, instance, parameterNames);

            var anyEqual = methods.Any(t => !AreMethodsEqual(t.method, method));
            
            if (anyEqual)
            {
                Debug.LogError($"Method {method.Name} already exists in the list of commands");
                return;
            }
            
            int commandIndex = FindConsoleMethodInfoIndex(consoleMethodInfo);
            methods.Insert(commandIndex, consoleMethodInfo);
        }

        private static ConsoleMethodInfo CreateConsoleMethodInfo(string command, string description, MethodInfo method,
            object instance, string[] parameterNames)
        {
            ConsoleMethodInfo consoleMethodInfo = null;
            if (string.IsNullOrEmpty(command))
            {
                Debug.LogError("Command name can't be empty!");
                return consoleMethodInfo;
            }

            command = command.Trim();
            if (command.IndexOf(' ') >= 0)
            {
                Debug.LogError("Command name can't contain whitespace: " + command);
                return consoleMethodInfo;
            }
            
            
            ParameterInfo[] parameters = method.GetParameters();
            
            var parameterTypes = GetParameterTypes(parameters);

            // Create the command
            string methodSignature = CreateMethodSignature(command, description, parameterNames, parameterTypes, parameters, out var parameterSignatures);

            consoleMethodInfo = new ConsoleMethodInfo(method, parameterTypes, instance, command,
                methodSignature,
                parameterSignatures);
            
            return consoleMethodInfo;
        }

        private static Type[] GetParameterTypes(ParameterInfo[] parameters)
        {
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    Debug.LogError("Command can't have 'out' or 'ref' parameters");
                    return parameterTypes;
                }

                Type parameterType = parameters[i].ParameterType;
                if (CommandTypeParser.HasParserForType(parameterType) ||
                    typeof(Component).IsAssignableFrom(parameterType) ||
                    parameterType.IsEnum || CommandTypeParser.IsSupportedArrayType(parameterType))
                    parameterTypes[i] = parameterType;
                else
                {
                    Debug.LogError(string.Concat("Parameter ", parameters[i].Name, "'s Type ", parameterType,
                        " isn't supported"));
                    return parameterTypes;
                }
            }

            return parameterTypes;
        }

        private static string CreateMethodSignature(string command, string description, string[] parameterNames,
            Type[] parameterTypes, ParameterInfo[] parameters, out string[] parameterSignatures)
        {
            string ms;
            StringBuilder methodSignature = new StringBuilder(256);
            parameterSignatures = new string[parameterTypes.Length];

            methodSignature.Append(command);

            if (parameterTypes.Length > 0)
            {
                methodSignature.Append(" ");

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    int parameterSignatureStartIndex = methodSignature.Length;

                    methodSignature.Append("[").Append(GetTypeReadableName(parameterTypes[i])).Append(" ").Append(
                        (parameterNames != null && i < parameterNames.Length &&
                         !string.IsNullOrEmpty(parameterNames[i]))
                            ? parameterNames[i]
                            : parameters[i].Name).Append("]");

                    if (i < parameterTypes.Length - 1)
                        methodSignature.Append(" ");

                    parameterSignatures[i] = methodSignature.ToString(parameterSignatureStartIndex,
                        methodSignature.Length - parameterSignatureStartIndex);
                }
            }

            if (!string.IsNullOrEmpty(description))
                methodSignature.Append(": ").Append(description);
            ms = methodSignature.ToString();
            return ms;
        }

        private static int FindConsoleMethodInfoIndex(ConsoleMethodInfo consoleMethodInfo)
        {
            string command = consoleMethodInfo.command;
            Type[] parameterTypes = consoleMethodInfo.parameterTypes;

            int commandIndex =
                CachedMethodFinder.FindCommandIndex(command, methods.AsReadOnly(), caseInsensitiveComparer);
            if (commandIndex < 0)
                commandIndex = ~commandIndex;
            else
            {
                int commandFirstIndex = commandIndex;
                int commandLastIndex = commandIndex;

                while (commandFirstIndex > 0 && caseInsensitiveComparer.Compare(methods[commandFirstIndex - 1].command,
                           command, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandFirstIndex--;
                while (commandLastIndex < methods.Count - 1 && caseInsensitiveComparer.Compare(
                           methods[commandLastIndex + 1].command, command,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandLastIndex++;

                commandIndex = commandFirstIndex;
                for (int i = commandFirstIndex; i <= commandLastIndex; i++)
                {
                    int parameterCountDiff = methods[i].parameterTypes.Length - parameterTypes.Length;
                    if (parameterCountDiff <= 0)
                    {
                        commandIndex = i + 1;
                        if (parameterCountDiff == 0)
                        {
                            int j = 0;
                            while (j < parameterTypes.Length && parameterTypes[j] == methods[i].parameterTypes[j])
                                j++;

                            if (j >= parameterTypes.Length)
                            {
                                commandIndex = i;
                                commandLastIndex--;
                            }
                        }
                    }
                }
            }

            return commandIndex;
        }

        // Remove all commands with the matching command name from the console
        public void RemoveCommand(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                for (int i = methods.Count - 1; i >= 0; i--)
                {
                    if (caseInsensitiveComparer.Compare(methods[i].command, command,
                            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                        methods.RemoveAt(i);
                }
            }
        }

        // Returns the first command that starts with the entered argument
        public string GetAutoCompleteCommand(string commandStart, string previousSuggestion)
        {
            int commandIndex =
                CachedMethodFinder.FindCommandIndex(
                    !string.IsNullOrEmpty(previousSuggestion) ? previousSuggestion : commandStart, methods.AsReadOnly(),
                    caseInsensitiveComparer);
            if (commandIndex < 0)
            {
                commandIndex = ~commandIndex;
                return (commandIndex < methods.Count && caseInsensitiveComparer.IsPrefix(methods[commandIndex].command,
                    commandStart, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
                    ? methods[commandIndex].command
                    : null;
            }

            // Find the next command that starts with commandStart and is different from previousSuggestion
            for (int i = commandIndex + 1; i < methods.Count; i++)
            {
                if (caseInsensitiveComparer.Compare(methods[i].command, previousSuggestion,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    continue;
                else if (caseInsensitiveComparer.IsPrefix(methods[i].command, commandStart,
                             CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
                    return methods[i].command;
                else
                    break;
            }

            // Couldn't find a command that follows previousSuggestion and satisfies commandStart, loop back to the beginning of the autocomplete suggestions
            string result = null;
            for (int i = commandIndex - 1;
                 i >= 0 && caseInsensitiveComparer.IsPrefix(methods[i].command, commandStart,
                     CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
                 i--)
                result = methods[i].command;

            return result;
        }

        /// <summary>
        /// Parse the command and try to execute it
        /// </summary>
        /// <param name="command"></param>
        public void TryExecuteCommand(string command)
        {
            string objectName;
            List<GameObject> gameobjectsWithCorrectName = new List<GameObject>();
            bool isGameObject = command.StartsWith(MONOCOMMANDINDICATOR);
            if (isGameObject)
            {
                command = command.Substring(1);
            }

            command = command.Trim();

            if (command.Length == 0 || string.IsNullOrEmpty(command))
            {
                return;
            }

            // Split the command's arguments
            commandArguments.Clear();
            CommandTypeParser.SplitIntoArgumentsForCommand(command, commandArguments);

            if (isGameObject)
            {
                objectName = commandArguments.Last();
                commandArguments.Remove(commandArguments.Last());

                gameobjectsWithCorrectName = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID)
                    .Where(t => t.name == objectName).ToList();
                if (gameobjectsWithCorrectName.Count == 0)
                {
                    Debug.LogWarning("Couldn't find object with name: " + objectName);
                    return;
                }
            }

            // Find all matching commands
            matchingMethods.Clear();

            bool parameterCountMismatch = false;
            int commandIndex =
                CachedMethodFinder.FindCommandIndex(commandArguments[0], methods, caseInsensitiveComparer);
            
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
                FindCommands(_command, !parameterCountMismatch, matchingMethods);

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

                    // The log that lists method signature(s) for this command should automatically be expanded for better UX
                    LongMessageAdded?.Invoke();
                    //DebugLogManager.Instance.AdjustLatestPendingLog(true, true);
                }

                return;
            }

            var methodToExecute = FindMatchingMethod(out var parameters);

            if (methodToExecute == null)
                Debug.LogWarning($"ERROR: something went wrong, could not find method to execute for command {command}");
            else
            {
                if (isGameObject)
                {
                    var type = methodToExecute.method.DeclaringType;
                    //Get the first monoObject as type
                    var obj = gameobjectsWithCorrectName[0].GetComponent(type);

                    RegisterObjectInstance(obj);
                }

                ExecuteInstanceMethodIfAvailable(methodToExecute, parameters);
            }
        }

        private ConsoleMethodInfo FindMatchingMethod(out object[] parameters)
        {
            ConsoleMethodInfo methodToExecute = null;
            parameters = new object[commandArguments.Count - 1];
            string errorMessage = null;
            for (int i = 0; i < matchingMethods.Count && methodToExecute == null; i++)
            {
                ConsoleMethodInfo methodInfo = matchingMethods[i];

                // Parse the parameters into objects
                bool success = true;
                for (int j = 0; j < methodInfo.parameterTypes.Length && success; j++)
                {
                    try
                    {
                        string argument = commandArguments[j + 1];
                        Type parameterType = methodInfo.parameterTypes[j];

                        object val;
                        if (CommandTypeParser.ParseArgument(argument, parameterType, out val))
                            parameters[j] = val;
                        else
                        {
                            success = false;
                            errorMessage = string.Concat("ERROR: couldn't parse ", argument, " to ",
                                GetTypeReadableName(parameterType));
                        }
                    }
                    catch (Exception e)
                    {
                        success = false;
                        errorMessage = "ERROR: " + e.ToString();
                    }
                }

                if (success)
                    methodToExecute = methodInfo;
            }

            return methodToExecute;
        }

        private static void ExecuteInstanceMethodIfAvailable(ConsoleMethodInfo methodToExecute, object[] parameters)
        {
            methodToExecute.TryGetInstnace(out object instance);

            if (instance == null && IsMethodInMonoBehaviourSubclass(methodToExecute.method))
            {
                var type = methodToExecute.method.DeclaringType;
                var obj = GameObject.FindFirstObjectByType(type);

                if (obj != null)
                {
                    RegisterObjectInstance(obj);
                    methodToExecute.TryGetInstnace(out instance);
                }
            }

            // Execute the method associated with the command
            object result = methodToExecute.method.Invoke(instance, parameters);
            if (methodToExecute.method.ReturnType != typeof(void))
            {
                // Print the returned value to the console
                if (result == null || result.Equals(null))
                    Debug.Log("Returned: null");
                else
                    Debug.Log("Returned: " + result.ToString());
            }
        }


        internal static void FindCommands(string commandName, bool allowSubstringMatching,
            List<ConsoleMethodInfo> matchingCommands)
        {
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
        }


        private static bool IsMethodInMonoBehaviourSubclass(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            Type declaringType = methodInfo.DeclaringType;

            // Check if the declaring type is not null and is a subclass of MonoBehaviour
            return declaringType != null && declaringType.IsSubclassOf(typeof(MonoBehaviour));
        }


        // Find command's index in the list of registered commands using binary search


        public static string GetTypeReadableName(Type type)
        {
            string result;
            if (ReadableTypes.TryGetReadableName(type, out result))
                return result;

            if (CommandTypeParser.IsSupportedArrayType(type))
            {
                Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                if (ReadableTypes.TryGetReadableName(elementType, out result))
                    return result + "[]";
                else
                    return elementType.Name + "[]";
            }

            return type.Name;
        }

        internal static List<ConsoleMethodInfo> GetMethods()
        {
            return methods;
        }

        public static async void FindCommandsStartingWithAsync(string trimStart)
        {
        }
    }
}