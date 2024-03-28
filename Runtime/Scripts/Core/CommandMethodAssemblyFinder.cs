using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DragynGames.Console
{
    internal class CommandMethodAssemblyFinder
    {
        internal List<CommandDefinitionData> SearchForCommands()
        {
            List<CommandDefinitionData> foundMethods = new();
            List<MethodInfo> methodsToAdd = new();

            List<Assembly> assemblies = UnityBuiltInAssemblyIgnorer.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Static |
                                                                      BindingFlags.Instance |
                                                                      BindingFlags.Public |
                                                                      BindingFlags.NonPublic |
                                                                      BindingFlags.DeclaredOnly))
                        {
                            methodsToAdd.Add(method);
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

            foreach (var methodInfo in methodsToAdd)
            {
                foundMethods.Add(GetCommandDefinitionData(methodInfo));
            }

            return foundMethods;
        }

        private CommandDefinitionData GetCommandDefinitionData(MethodInfo method)
        {
            CommandDefinitionData result = null;
            foreach (object attribute in method.GetCustomAttributes(typeof(ConsoleActionAttribute),
                         false))
            {
                ConsoleActionAttribute consoleActionAttribute = attribute as ConsoleActionAttribute;
                if (consoleActionAttribute != null)
                {
                    var commandType = FindCommandType(method);

                    result = new CommandDefinitionData(consoleActionAttribute, method, commandType);
                }
            }

            return result;
        }
        
        public static List<MethodInfo> GetInstanceMethods(Type type)
        {
            List<MethodInfo> methods = new();
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance |
                                                          BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.DeclaredOnly))
            {
                methods.Add(method);
            }

            return methods;
        }

        internal static CommandType FindCommandType(MethodInfo method)
        {
            CommandType commandType = Console.CommandType.Invalid;
            if (IsMethodInMonoBehaviourSubclass(method))
            {
                commandType = Console.CommandType.MonoBehaviour;
            }
            else if (!method.IsStatic)
            {
                commandType = Console.CommandType.Instanced;
            }
            else
            {
                commandType = Console.CommandType.Static;
            }

            return commandType;
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

        private static bool IsMethodInMonoBehaviourSubclass(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            Type declaringType = methodInfo.DeclaringType;

            // Check if the declaring type is not null and is a subclass of MonoBehaviour
            return declaringType != null && declaringType.IsSubclassOf(typeof(MonoBehaviour));
        }
    }
}