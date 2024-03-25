#if UNITY_EDITOR || UNITY_STANDALONE
// Unity's Text component doesn't render <b> tag correctly on mobile devices
#define USE_BOLD_COMMAND_SIGNATURES
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Object = UnityEngine.Object;
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using SystemInfo = UnityEngine.Device.SystemInfo; // To support Device Simulator on Unity 2021.1+
#endif

// Manages the console commands, parses console input and handles execution of commands
// Supported method parameter types: int, float, bool, string, Vector2, Vector3, Vector4

// Helper class to store important information about a command
namespace DragynGames.Console
{
    public class MethodHandler
    {
        public static event Action LongMessageAdded;

        public delegate bool ParseFunction(string input, out object output);

        // All the commands
        private static readonly List<ConsoleMethodInfo> methods = new List<ConsoleMethodInfo>();
        private static readonly List<ConsoleMethodInfo> matchingMethods = new List<ConsoleMethodInfo>(4);

        // Split arguments of an entered command
        private readonly List<string> commandArguments = new List<string>(8);


        // CompareInfo used for case-insensitive command name comparison
        internal static readonly CompareInfo caseInsensitiveComparer = new CultureInfo("en-US").CompareInfo;

        private CommandTypeParser _commandTypeParser;

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
            SearchForCommands();
        }

        private void SearchForCommands()
        {
            List<Assembly> assemblies = UnityBuiltInAssemblyIgnorer.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Static |
                                                                      BindingFlags.Public |
                                                                      BindingFlags.NonPublic |
                                                                      BindingFlags.DeclaredOnly))
                        {
                            AddMethodAsCommand(method);
                        }

                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance |
                                                                      BindingFlags.Public |
                                                                      BindingFlags.NonPublic |
                                                                      BindingFlags.DeclaredOnly))
                        {
                            AddMethodAsCommand(method, true);
                        }
                    }
                }
                catch (NotSupportedException)
                {
                }
                catch (System.IO.FileNotFoundException)
                {
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogError("Couldn't search assembly for [ConsoleMethod] attributes: " + assembly.FullName +
                                   "\n" +
                                   e.ToString());
                }
            }
        }

        private void AddMethodAsCommand(MethodInfo method, bool instanced = false)
        {
            foreach (object attribute in method.GetCustomAttributes(typeof(ConsoleActionAttribute),
                         false))
            {
                ConsoleActionAttribute consoleMethod = attribute as ConsoleActionAttribute;
                if (consoleMethod != null)
                {
                    if (string.IsNullOrEmpty(consoleMethod.Command))
                    {
                        ParameterInfo[] paramInfos = method.GetParameters();

                        string[] parameters = paramInfos.Length > 0 ? new string[paramInfos.Length] : null;

                        for (int i = 0; i < paramInfos.Length; i++)
                        {
                            ParameterInfo paramInfo = paramInfos[i];
                            parameters[i] = paramInfo.Name;
                        }

                        consoleMethod.SetField(method.Name, "", parameters);
                    }

                    AddCommand(consoleMethod.Command, consoleMethod.Description, method, instanced, null,
                        consoleMethod.ParameterNames);
                }
            }
        }


        // Add a custom Type to the list of recognized command parameter Types


        // Remove a custom Type from the list of recognized command parameter Types
        public void RemoveCustomParameterType(Type type)
        {
            _commandTypeParser.RemoveType(type);
            ReadableTypes.Remove(type);
        }

        #region AddCommand overloads

        // Add a command related with a static method (i.e. no instance is required to call the method)
        public void AddCommandStatic(string command, string description, string methodName, Type ownerType,
            params string[] parameterNames)
        {
            AddCommand(command, description, methodName, ownerType, null, parameterNames);
        }

        // Add a command that can be related to either a static or an instance method
        public static void AddCommand(string command, string description, Action method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1>(string command, string description, Action<T1> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1>(string command, string description, Func<T1> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2>(string command, string description, Action<T1, T2> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2>(string command, string description, Func<T1, T2> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2, T3>(string command, string description, Action<T1, T2, T3> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2, T3>(string command, string description, Func<T1, T2, T3> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2, T3, T4>(string command, string description, Action<T1, T2, T3, T4> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2, T3, T4>(string command, string description, Func<T1, T2, T3, T4> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand<T1, T2, T3, T4, T5>(string command, string description,
            Func<T1, T2, T3, T4, T5> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public void AddCommand(string command, string description, Delegate method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        // Add a command with custom parameter names
        public void AddCommand<T1>(string command, string description, Action<T1> method, bool instanced,
            string parameterName)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, new string[1] {parameterName});
        }

        public void AddCommand<T1, T2>(string command, string description, Action<T1, T2> method, bool instanced,
            string parameterName1, string parameterName2)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[2] {parameterName1, parameterName2});
        }

        public void AddCommand<T1, T2>(string command, string description, Func<T1, T2> method, bool instanced,
            string parameterName)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, new string[1] {parameterName});
        }

        public void AddCommand<T1, T2, T3>(string command, string description, Action<T1, T2, T3> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[3] {parameterName1, parameterName2, parameterName3});
        }

        public void AddCommand<T1, T2, T3>(string command, string description, Func<T1, T2, T3> method, bool instanced,
            string parameterName1, string parameterName2)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[2] {parameterName1, parameterName2});
        }

        public void AddCommand<T1, T2, T3, T4>(string command, string description, Action<T1, T2, T3, T4> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3, string parameterName4)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[4] {parameterName1, parameterName2, parameterName3, parameterName4});
        }

        public void AddCommand<T1, T2, T3, T4>(string command, string description, Func<T1, T2, T3, T4> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[3] {parameterName1, parameterName2, parameterName3});
        }

        public void AddCommand<T1, T2, T3, T4, T5>(string command, string description,
            Func<T1, T2, T3, T4, T5> method, bool instanced, string parameterName1, string parameterName2,
            string parameterName3,
            string parameterName4)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[4] {parameterName1, parameterName2, parameterName3, parameterName4});
        }

        public void AddCommand(string command, string description, bool instanced, Delegate method,
            params string[] parameterNames)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, parameterNames);
        }

        #endregion

        // Create a new command and set its properties
        private void AddCommand(string command, string description, string methodName, Type ownerType,
            object instance, string[] parameterNames)
        {
            // Get the method from the class
            MethodInfo method = ownerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic |
                (instance != null ? BindingFlags.Instance : BindingFlags.Static));
            if (method == null)
            {
                Debug.LogError(methodName + " does not exist in " + ownerType);
                return;
            }

            bool instanced = !method.IsStatic;

            AddCommand(command, description, method, instanced, instance, parameterNames);
        }

        private static void AddCommand(string command, string description, MethodInfo method, bool instanced,
            object instance,
            string[] parameterNames)
        {
            if (string.IsNullOrEmpty(command))
            {
                Debug.LogError("Command name can't be empty!");
                return;
            }

            command = command.Trim();
            if (command.IndexOf(' ') >= 0)
            {
                Debug.LogError("Command name can't contain whitespace: " + command);
                return;
            }

            // Fetch the parameters of the class
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters == null)
                parameters = new ParameterInfo[0];

            // Store the parameter types in an array
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    Debug.LogError("Command can't have 'out' or 'ref' parameters");
                    return;
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
                    return;
                }
            }

            int commandIndex = FindCommandIndex(command);
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
                        // We are sorting the commands in 2 steps:
                        // 1: Sorting by their 'command' names which is handled by FindCommandIndex
                        // 2: Sorting by their parameter counts which is handled here (parameterCountDiff <= 0)
                        commandIndex = i + 1;

                        // Check if this command has been registered before and if it is, overwrite that command
                        if (parameterCountDiff == 0)
                        {
                            int j = 0;
                            while (j < parameterTypes.Length && parameterTypes[j] == methods[i].parameterTypes[j])
                                j++;

                            if (j >= parameterTypes.Length)
                            {
                                commandIndex = i;
                                commandLastIndex--;
                                methods.RemoveAt(i--);

                                continue;
                            }
                        }
                    }
                }
            }

            // Create the command
            StringBuilder methodSignature = new StringBuilder(256);
            string[] parameterSignatures = new string[parameterTypes.Length];

#if USE_BOLD_COMMAND_SIGNATURES
            methodSignature.Append("<b>");
#endif
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

#if USE_BOLD_COMMAND_SIGNATURES
            methodSignature.Append("</b>");
#endif

            if (!string.IsNullOrEmpty(description))
                methodSignature.Append(": ").Append(description);

            methods.Insert(commandIndex,
                new ConsoleMethodInfo(method, parameterTypes, instance, command, methodSignature.ToString(),
                    parameterSignatures));
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


        #region RemoveCommand overloads

        public void RemoveCommand<T1>(Action<T1> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1>(Func<T1> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2>(Action<T1, T2> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2>(Func<T1, T2> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2, T3>(Action<T1, T2, T3> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2, T3>(Func<T1, T2, T3> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2, T3, T4>(Func<T1, T2, T3, T4> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand(Delegate method)
        {
            RemoveCommand(method.Method);
        }

        public void RemoveCommand(MethodInfo method)
        {
            if (method != null)
            {
                for (int i = methods.Count - 1; i >= 0; i--)
                {
                    if (methods[i].method == method)
                        methods.RemoveAt(i);
                }
            }
        }

        #endregion

        // Returns the first command that starts with the entered argument
        public string GetAutoCompleteCommand(string commandStart, string previousSuggestion)
        {
            int commandIndex =
                FindCommandIndex(!string.IsNullOrEmpty(previousSuggestion) ? previousSuggestion : commandStart);
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

        // Parse the command and try to execute it
        public void TryExecuteCommand(string command)
        {
            if (command == null)
                return;

            command = command.Trim();

            if (command.Length == 0)
                return;

            // Split the command's arguments
            commandArguments.Clear();
            CommandTypeParser.FetchArgumentsFromCommand(command, commandArguments);

            // Find all matching commands
            matchingMethods.Clear();
            bool parameterCountMismatch = false;
            int commandIndex = FindCommandIndex(commandArguments[0]);
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

            ConsoleMethodInfo methodToExecute = null;
            object[] parameters = new object[commandArguments.Count - 1];
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

            if (methodToExecute == null)
                Debug.LogWarning(!string.IsNullOrEmpty(errorMessage) ? errorMessage : "ERROR: something went wrong");
            else
            {
                ExecuteInstanceMethodIfAvailable(methodToExecute, parameters);
            }
        }

        private static void ExecuteInstanceMethodIfAvailable(ConsoleMethodInfo methodToExecute, object[] parameters)
        {
            methodToExecute.TryGetInstnace(out object instance);

            if(instance == null && IsMethodInMonoBehaviourSubclass(methodToExecute.method))
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


        public static void FindCommands(string commandName, bool allowSubstringMatching,
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

        // Finds all commands that have a matching signature with command
        // - caretIndexIncrements: indices inside "string command" that separate two arguments in the command. This is used to
        //   figure out which argument the caret is standing on
        // - commandName: command's name (first argument)
        internal void GetCommandSuggestions(string command, List<ConsoleMethodInfo> matchingCommands,
            List<int> caretIndexIncrements, ref string commandName, out int numberOfParameters)
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
                }
                else
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
                }

                numberOfParameters++;
            }

            if (!commandNameCalculated)
                commandName = string.Empty;

            if (!string.IsNullOrEmpty(commandName))
            {
                int commandIndex = FindCommandIndex(commandName);
                if (commandIndex < 0)
                    commandIndex = ~commandIndex;

                int commandLastIndex = commandIndex;
                if (!commandNameFullyTyped)
                {
                    // Match all commands that start with commandName
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
                }
                else
                {
                    // Match only the commands that are equal to commandName
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
                }

                for (; commandIndex <= commandLastIndex; commandIndex++)
                {
                    if (methods[commandIndex].parameterTypes.Length >= numberOfParameters)
                        matchingCommands.Add(methods[commandIndex]);
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
        private static int FindCommandIndex(string command)
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

        public static List<ConsoleMethodInfo> GetMethods()
        {
            return methods;
        }
    }
}