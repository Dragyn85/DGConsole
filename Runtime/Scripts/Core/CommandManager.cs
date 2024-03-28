using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DragynGames.Console
{
    public partial class CommandManager
    {
        internal const char ObjectIdentifier = '@';
        private static List<ConsoleMethodInfo> methods;
        CommandTypeParser _commandTypeParser;

        internal static readonly CompareInfo caseInsensitiveComparer = new CultureInfo("en-US").CompareInfo;

        public CommandManager()
        {
            _commandTypeParser = new CommandTypeParser();

            CommandMethodAssemblyFinder commandMethodAssemblyFinder = new CommandMethodAssemblyFinder();
            var commandDefinitionDatas = commandMethodAssemblyFinder.SearchForCommands();
        }

        public static void RegisterObjectInstance(object consoleAction)
        {
            var instance = consoleAction;
            Type type = instance.GetType();

            List<MethodInfo> methodsInType = CommandMethodAssemblyFinder.GetInstanceMethods(type);
            
            foreach (var method in methodsInType)
            {
                methods.Where(t => t.method == method).ToList().ForEach(t => t.SetInstance(instance));
            }
        }

        private static void AddCommand(
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

            var consoleMethodInfo = new ConsoleMethodInfo(method, parameterTypes, command,
                methodSignature, parameterSignatures,commandDefinitionData.instance, commandDefinitionData.CommandType);

            var anyEqual = methods.Any(t => !CommandMethodAssemblyFinder.AreMethodsEqual(t.method, method));

            if (anyEqual)
            {
                Debug.LogError($"Method {method.Name} already exists in the list of commands");
                return;
            }
            
            int commandIndex = FindConsoleMethodInfoIndex(consoleMethodInfo);
            methods.Insert(commandIndex, consoleMethodInfo);
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
                CommandTypeParser.SplitIntoArgumentsForCommand(command, out targetObjectName, ObjectIdentifier);

            //Find the method in the list of commands
            int index = CachedMethodFinder.FindCommandIndex(argumentsForCommand[0], methods, caseInsensitiveComparer);

            var matchingMethods =
                CachedMethodFinder.GetMatchingMethods(argumentsForCommand, methods, caseInsensitiveComparer);
            var methodToExecute = FindMatchingMethod(out var parameters, argumentsForCommand, matchingMethods);
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
                    results = TryFindGameObject(methodToExecute.method.DeclaringType, targetObjectName, out targetObject);
                    break;
                }
            }
            
            if (results)
            {
                commandExecutionResult.ReturnedObject = methodToExecute.method.Invoke(targetObject, parameters);
            }
            else
            {
                commandExecutionResult.ExecutionMessage = "Failed to execute command";
            }
            

            //Execute the method with depending on the type of method

            return results;
        }

        private static bool TryFindGameObject(Type type, string targetObjectName,out object targetObject)
        {
            bool results = true;
            targetObject = null;
            
            if (string.IsNullOrEmpty(targetObjectName))
            {
                GameObject gameObjectOfType = GameObject.FindObjectOfType(type) as GameObject;

                if (gameObjectOfType != null)
                {
                    targetObject = gameObjectOfType.GetComponent(type);
                }
                else
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

        private ConsoleMethodInfo FindMatchingMethod(out object[] parameters, List<string> commandArguments,
            List<ConsoleMethodInfo> matchingMethods)
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
}