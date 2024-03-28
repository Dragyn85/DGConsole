using System;
using System.Reflection;
using UnityEngine;

namespace DragynGames.Commands
{
    public partial class CommandManager
    {
        private static void AddCommand(string description, string methodName, Type ownerType,
            object instance, string[] parameterNames)
        {
            // Get the method from the class
            MethodInfo method = ownerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError(methodName + " does not exist in " + ownerType);
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

        public static void AddCommand(string command, string description, bool instanced, Delegate method,
            params string[] parameterNames)
        {
            // Get the method from the class
            MethodInfo methodInfo = method.Method;

            if (methodInfo == null)
            {
                Debug.LogError(method.Method.Name + " does not exist in " + method.Target.GetType());
                return;
            }

            // Check if the number of parameter names matches the number of parameters in the method
            if (methodInfo.GetParameters().Length != parameterNames.Length)
            {
                Debug.LogError("The number of parameter names does not match the number of parameters in the method.");
                return;
            }

            CommandType commandType = CommandMethodAssemblyFinder.FindCommandType(methodInfo);
            ManualCommandCreationInfo info = new ManualCommandCreationInfo
            {
                commandName = command,
                description = description
            };
            CommandDefinitionData commandDefinitionData =
                new CommandDefinitionData(null, methodInfo, commandType, info);
            AddCommand(parameterNames, commandDefinitionData);
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
    }

    internal struct ManualCommandCreationInfo
    {
        public string commandName;
        public string description;
        public object instance;
    }
}