using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DragynGames.Commands
{
    public class CommandManager
    {
        private List<CommandInfo> sortedCommands = new List<CommandInfo>();
        private CommandTypeParser _commandTypeParser;
        private ConsoleBuiltInActions _consoleBuiltInActions;
        public CommandSystemSettings _settings = new CommandSystemSettings();
        private CachedMethodFinder cachedMethodFinder = new CachedMethodFinder();

        private CancellationTokenSource codeCompletionCancellationTokenSource;
        private int msForCodeCompletion = 150;


        public CommandManager(bool useBuiltInCommands = true)

        {
            _commandTypeParser = new CommandTypeParser();

            CommandMethodAssemblyFinder commandMethodAssemblyFinder = new CommandMethodAssemblyFinder();
            List<CommandDefinitionData> commandDefinitionDatas = commandMethodAssemblyFinder.SearchForCommands();
            foreach (var co in commandDefinitionDatas)
            {
                AddCommand(co.Parameters, co);
            }

            if (useBuiltInCommands)
            {
                _consoleBuiltInActions = new ConsoleBuiltInActions(this);
                _consoleBuiltInActions.AddHelpCommands();
            }
        }

        public void GetSuggestions(string commands, Action<List<string>> callback)
        {
            // Cancel the previous task
            if (codeCompletionCancellationTokenSource != null)
            {
                codeCompletionCancellationTokenSource.Cancel();
                codeCompletionCancellationTokenSource.Dispose();
            }

            // Create a new CancellationTokenSource
            codeCompletionCancellationTokenSource = new CancellationTokenSource();

            // Start a new task without awaiting it
            _ = cachedMethodFinder.GetCommandSuggestionsAsync(commands, sortedCommands,
                _settings.caseInsensitiveComparer, commands, callback, msForCodeCompletion,
                codeCompletionCancellationTokenSource.Token);
        }

        public void RegisterObjectInstance(object consoleAction)
        {
            var instance = consoleAction;
            Type type = instance.GetType();

            List<MethodInfo> methodsInType = CommandMethodAssemblyFinder.GetInstanceMethods(type);

            foreach (var method in methodsInType)
            {
                sortedCommands.Where(t => t.method == method).ToList().ForEach(t => t.SetInstance(instance));
            }
        }

        private void AddCommand(
            string[] parameterNames, CommandDefinitionData commandDefinitionData)
        {
            var command = commandDefinitionData.Command;
            var method = commandDefinitionData.MethodInfo;
            if (string.IsNullOrEmpty(commandDefinitionData.Command))
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

            ParameterInfo[] parameters = method.GetParameters();

            var parameterTypes = GetParameterTypes(parameters);

            string methodSignature = CreateMethodSignature(command, commandDefinitionData.Description,
                parameterNames, parameterTypes, parameters, out var parameterSignatures);

            var consoleMethodInfo = new CommandInfo(method, parameterTypes, command,
                methodSignature, parameterSignatures, commandDefinitionData.instance,
                commandDefinitionData.CommandType);

            var anyEqual = sortedCommands.Any(t => CommandMethodAssemblyFinder.AreMethodsEqual(t.method, method));

            if (anyEqual)
            {
                Debug.LogError($"Method {method.Name} already exists in the list of commands");
                return;
            }

            int commandIndex = FindCommandInfoIndex(consoleMethodInfo);
            sortedCommands.Insert(commandIndex, consoleMethodInfo);
        }

        public void AddCommand(string methodName, string description, Type ownerType,
            object instance, string[] parameterNames)
        {
            // Get the method from the class
            MethodInfo method = ownerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            CommandType commandType = CommandMethodAssemblyFinder.FindCommandType(method);
            ManualCommandCreationInfo info = new ManualCommandCreationInfo
            {
                description = description,
                instance = instance
            };

            CommandDefinitionData commandDefinitionData =
                new CommandDefinitionData(null, method, commandType, info);
            AddCommand(parameterNames, commandDefinitionData);
        }

        public void AddCommand(string command, string description, object callerObject, Delegate method,
            params string[] parameterNames)
        {
            // Get the method from the class
            MethodInfo methodInfo = method.Method;

            if (methodInfo == null)
            {
                Debug.LogError(method.Method.Name + " does not exist in " + method.Target.GetType());
                return;
            }

            // Get the delegate's parameter names
            var delegateParameterNames = methodInfo.GetParameters().Select(p => p.Name).ToArray();
            string[] finalParameterNames = new string[delegateParameterNames.Length];

            // If there are too few parameterNames, use the delegate's parameter names
            if (parameterNames.Length < delegateParameterNames.Length)
            {
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    finalParameterNames[i] = parameterNames[i];
                }

                for (int i = parameterNames.Length; i < delegateParameterNames.Length; i++)
                {
                    finalParameterNames[i] = delegateParameterNames[i];
                }
            }
            else if (parameterNames.Length > delegateParameterNames.Length)
            {
                Debug.LogError("Too many parameter names for " + method.Method.Name);
                return;
            }
            else
            {
                finalParameterNames = parameterNames;
            }

            CommandType commandType = CommandMethodAssemblyFinder.FindCommandType(methodInfo);
            ManualCommandCreationInfo info = new ManualCommandCreationInfo
            {
                commandName = command,
                description = description,
                instance = callerObject
            };
            CommandDefinitionData commandDefinitionData =
                new CommandDefinitionData(null, methodInfo, commandType, info);
            AddCommand(finalParameterNames, commandDefinitionData);
        }


        public void RemoveCommand(Delegate method) => RemoveCommand(method.Method);

        public void RemoveCommand(MethodInfo method)
        {
            if (method != null)
            {
                for (int i = sortedCommands.Count - 1; i >= 0; i--)
                {
                    if (sortedCommands[i].method == method)
                        sortedCommands.RemoveAt(i);
                }
            }
        }

        public bool ExecuteMethod(string command, out CommandExecutionResult commandExecutionResult)
        {
            commandExecutionResult = new CommandExecutionResult();
            bool results = true;

            command = command.Trim();
            string targetObjectName;

            if (string.IsNullOrEmpty(command))
                return false;


            //Parse string command to get the method name and parameters
            var argumentsForCommand =
                CommandTypeParser.SplitIntoArgumentsForCommand(command, out targetObjectName,
                    _settings.ObjectIdentifier);

            var matchingMethods =
                CachedMethodFinder.GetMatchingMethods(argumentsForCommand, sortedCommands,
                    _settings.caseInsensitiveComparer);

            var methodToExecute = FindMatchingMethod(out var parameters, argumentsForCommand, matchingMethods);
            if (methodToExecute == null)
            {
                commandExecutionResult.ExecutionMessage = "Command not found";
                return false;
            }

            object targetObject = null;
            //Determines if the method is static, instanced or a MonoBehaviour
            switch (methodToExecute.commandType)
            {
                case CommandType.Invalid:
                {
                    Debug.LogError("Invalid command type");
                    results = false;
                    commandExecutionResult.ExecutionMessage = "Invalid command type";
                    break;
                }
                case CommandType.Static:
                {
                    break;
                }
                case CommandType.Instanced:
                {
                    results = methodToExecute.TryGetInstnace(out targetObject);
                    break;
                }
                case CommandType.MonoBehaviour:
                {
                    if (methodToExecute.method.IsStatic)
                    {
                        results = true;
                    }
                    else
                    {
                        results = TryFindGameObject(methodToExecute.method.DeclaringType, targetObjectName,
                            out targetObject);
                    }

                    break;
                }
            }

            if (results)
            {
                commandExecutionResult.ReturnedObject = methodToExecute.method.Invoke(targetObject, parameters);
            }
            //Execute the method with depending on the type of method

            return results;
        }

        private bool TryFindGameObject(Type type, string targetObjectName, out object targetObject)
        {
            bool results = true;
            targetObject = null;

            if (string.IsNullOrEmpty(targetObjectName))
            {
                
                targetObject = FindTargetType(type);
                if (targetObject == null)
                {
                    results = false;
                }
            }
            else
            {
                var possibleTarget = GameObject.Find(targetObjectName);
                if (possibleTarget == null)
                {
                    results = false;
                }
                else
                {
                    var component = possibleTarget.GetComponent(type);
                    if (component == null)
                    {
                        var gameObject = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID)
                            .Where(t => t.name == targetObjectName).ToList()
                            .FirstOrDefault(t => t.TryGetComponent(out component));

                        if (component == null)
                        {
                            results = false;
                        }
                        else
                        {
                            targetObject = gameObject;
                        }
                    }
                    else
                    {
                        targetObject = component;
                    }
                }
            }

            return results;
        }

        

        private CommandInfo FindMatchingMethod(out object[] parameters, List<string> commandArguments,
            List<CommandInfo> matchingMethods)
        {
            CommandInfo methodToExecute = null;
            parameters = new object[commandArguments.Count - 1];
            string errorMessage = null;
            for (int i = 0; i < matchingMethods.Count && methodToExecute == null; i++)
            {
                CommandInfo methodInfo = matchingMethods[i];

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

        private Type[] GetParameterTypes(ParameterInfo[] parameters)
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

        private string CreateMethodSignature(string command, string description, string[] parameterNames,
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

        private int FindCommandInfoIndex(CommandInfo commandInfo)
        {
            string command = commandInfo.command;
            Type[] parameterTypes = commandInfo.parameterTypes;

            int commandIndex =
                CachedMethodFinder.FindFirstCommandIndex(command, sortedCommands.AsReadOnly(),
                    _settings.caseInsensitiveComparer);
            if (commandIndex < 0)
                commandIndex = ~commandIndex;
            else
            {
                int commandFirstIndex = commandIndex;
                int commandLastIndex = commandIndex;

                while (commandFirstIndex > 0 && _settings.caseInsensitiveComparer.Compare(
                           sortedCommands[commandFirstIndex - 1].command,
                           command, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandFirstIndex--;
                while (commandLastIndex < sortedCommands.Count - 1 && _settings.caseInsensitiveComparer.Compare(
                           sortedCommands[commandLastIndex + 1].command, command,
                           CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                    commandLastIndex++;

                commandIndex = commandFirstIndex;
                for (int i = commandFirstIndex; i <= commandLastIndex; i++)
                {
                    int parameterCountDiff = sortedCommands[i].parameterTypes.Length - parameterTypes.Length;
                    if (parameterCountDiff <= 0)
                    {
                        commandIndex = i + 1;
                        if (parameterCountDiff == 0)
                        {
                            int j = 0;
                            while (j < parameterTypes.Length &&
                                   parameterTypes[j] == sortedCommands[i].parameterTypes[j])
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
        
        private static Object FindTargetType(Type type)
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType(type);
#else
            return UnityEngine.Object.FindObjectOfType(type);
#endif
        }

        public string GetTypeReadableName(Type type)
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

        internal List<CommandInfo> GetMethods()
        {
            return sortedCommands;
        }

        public void FindCommandsStartingWithAsync(string trimStart)
        {
        }
    }

    internal enum CommandType
    {
        Invalid,
        Instanced,
        Static,
        MonoBehaviour
    }

    public struct CommandExecutionResult
    {
        public string ExecutionMessage;
        public object ReturnedObject;
    }

    public class CommandSystemSettings
    {
        public readonly char ObjectIdentifier = '@';
        public readonly CompareInfo caseInsensitiveComparer = new CultureInfo("en-US").CompareInfo;

        public CommandSystemSettings()
        {
        }

        public CommandSystemSettings(char objectIdentifier, CompareInfo compareInfo)
        {
            ObjectIdentifier = objectIdentifier;
            caseInsensitiveComparer = compareInfo;
        }
    }

    internal struct ManualCommandCreationInfo
    {
        public string commandName;
        public string description;
        public object instance;
    }
}