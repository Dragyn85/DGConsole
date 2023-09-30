using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DragynGames.Console
{
    public static class MethodHandler
    {

        public static event Action<List<MethodDescription>> OnSearchComplete;
        private static CancellationTokenSource cancellationTokenSource;

        private static AsyncOperation currentTask;

        public static async Task FindMethodsStartingAsync(string name)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var components = GameObject.FindObjectsOfType<Component>();
                List<MethodDescription> results = await Task.Run(() => Work(name, components, cancellationTokenSource.Token), cancellationTokenSource.Token);


                OnSearchComplete?.Invoke(results);
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e.Message);
            }
        }
        private static List<MethodDescription> Work(string name, Component[] components, CancellationToken cancellationToken)
        {
            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                // Perform any cleanup or handle cancellation-specific logic
                cancellationToken.ThrowIfCancellationRequested();
            }

            List<MethodDescription> results = FindMethodsStarting(name, components);
            return results;
        }
        public static List<MethodDescription> FindMethodsStarting(string name, Component[] components)
        {
            string searchTerm = name;

            Component[] withAttribute = FindComponentsWithAttribute(typeof(ConsoleAvailable), components);
            List<MethodDescription> methodDescriptions = new List<MethodDescription>();

            for (var i = 0; i < withAttribute.Length; i++)
            {
                Component component = withAttribute[i];
                Type type = component.GetType();
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (var j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    ConsoleAction[] attributes = (ConsoleAction[])method.GetCustomAttributes(typeof(ConsoleAction), false);
                    if (attributes.Length > 0 && (method.Name.StartsWith(searchTerm) || method.Name == searchTerm))
                    {
                        methodDescriptions.Add(CreatMethodDescription(method));
                    }
                }
            }

            return methodDescriptions;
        }

        private static MethodDescription CreatMethodDescription(MethodInfo method)
        {
            MethodDescription results;
            results.name = method.Name;
            ParameterInfo[] parameters = method.GetParameters();

            results.parameterName = new string[parameters.Length];
            results.parameterType = new string[parameters.Length];

            for (var k = 0; k < parameters.Length; k++)
            {
                ParameterInfo parameter = parameters[k];

                results.parameterName[k] = parameter.Name;
                results.parameterType[k] = parameter.ParameterType.ToString();
            }

            return results;
        }

        public static string ExecuteMethod(string inputString)
        {
            string answer = "";
            string[] methodParts = ParseInput(inputString);
            string methodName = methodParts[0];

            Component[] components = FindComponentsWithAttribute(typeof(ConsoleAvailable));

            for (var i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                Type type = component.GetType();
                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


                if (method != null)
                {
                    try
                    {
                        // Create an array to hold parameter values as objects
                        object[] methodParams = new object[methodParts.Length - 1];

                        for (int j = 0; j < methodParts.Length - 1; j++)
                        {
                            // Convert parameter values to appropriate types (you may need additional parsing logic)
                            methodParams[j] = ConvertParameter(methodParts[j + 1], method.GetParameters()[j].ParameterType);
                        }

                        // Invoke the method and get the return value
                        object returnValue = method.Invoke(component, methodParams);
                        if (returnValue != null)
                        {
                            answer = $"{returnValue}";
                        }
                    }
                    catch (Exception e)
                    {
                        answer = $"Error: {e.Message}";
                    }
                }
            }
            return answer;
        }

        private static Component[] FindComponentsWithAttribute(Type attribute, Component[] components)
        {
            Component[] componentsArray = components;
            List<Component> componentsList = new List<Component>();

            foreach (var component in componentsArray)
            {
                if (component != null && !component.Equals(null))
                {
                    object[] attributes = component.GetType().GetCustomAttributes(attribute, true);
                    if (attributes.Length > 0)
                    {
                        componentsList.Add(component);
                    }
                }
            }
            return componentsList.ToArray();
        }

        private static Component[] FindComponentsWithAttribute(Type attribute)
        {
            Component[] componentsArray = GameObject.FindObjectsOfType<Component>();
            return FindComponentsWithAttribute(attribute, componentsArray);
        }

        private static string[] ParseInput(string inputString)
        {
            char[] separators = new[] { '(', ',', ' ' };
            string[] executionParts = inputString.Split(separators);
            int lastIndex = executionParts.Length - 1;
            executionParts[lastIndex] = executionParts[lastIndex].TrimEnd(')');
            return executionParts;
        }

        private static object ConvertParameter(string value, Type targetType)
        {
            // Implement your own logic to convert the parameter value to the target type
            if (targetType == typeof(int))
            {
                return int.Parse(value);
            }
            else if (targetType == typeof(float))
            {
                return float.Parse(value);
            }
            // Add more type conversions as needed...

            // Default case: return as a string
            return value;
        }


    }
}

