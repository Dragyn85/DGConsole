using System;
using System.Reflection;
using UnityEngine;

namespace DragynGames.Console
{
    public partial class MethodHandler
    {
        #region AddCommand overloads

        private static void AddCommand(string command, string description, string methodName, Type ownerType,
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
        // Add a command related with a static method (i.e. no instance is required to call the method)
        public static void AddCommandStatic(string command, string description, string methodName, Type ownerType,
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

        public static void AddCommand<T1>(string command, string description, Func<T1> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2>(string command, string description, Action<T1, T2> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2>(string command, string description, Func<T1, T2> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2, T3>(string command, string description, Action<T1, T2, T3> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2, T3>(string command, string description, Func<T1, T2, T3> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2, T3, T4>(string command, string description, Action<T1, T2, T3, T4> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2, T3, T4>(string command, string description, Func<T1, T2, T3, T4> method,
            bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand<T1, T2, T3, T4, T5>(string command, string description,
            Func<T1, T2, T3, T4, T5> method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        public static void AddCommand(string command, string description, Delegate method, bool instanced)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, null);
        }

        // Add a command with custom parameter names
        public static void AddCommand<T1>(string command, string description, Action<T1> method, bool instanced,
            string parameterName)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, new string[1] {parameterName});
        }

        public static void AddCommand<T1, T2>(string command, string description, Action<T1, T2> method, bool instanced,
            string parameterName1, string parameterName2)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[2] {parameterName1, parameterName2});
        }

        public static void AddCommand<T1, T2>(string command, string description, Func<T1, T2> method, bool instanced,
            string parameterName)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, new string[1] {parameterName});
        }

        public static void AddCommand<T1, T2, T3>(string command, string description, Action<T1, T2, T3> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[3] {parameterName1, parameterName2, parameterName3});
        }

        public static void AddCommand<T1, T2, T3>(string command, string description, Func<T1, T2, T3> method,
            bool instanced,
            string parameterName1, string parameterName2)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[2] {parameterName1, parameterName2});
        }

        public static void AddCommand<T1, T2, T3, T4>(string command, string description, Action<T1, T2, T3, T4> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3, string parameterName4)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[4] {parameterName1, parameterName2, parameterName3, parameterName4});
        }

        public static void AddCommand<T1, T2, T3, T4>(string command, string description, Func<T1, T2, T3, T4> method,
            bool instanced,
            string parameterName1, string parameterName2, string parameterName3)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[3] {parameterName1, parameterName2, parameterName3});
        }

        public static void AddCommand<T1, T2, T3, T4, T5>(string command, string description,
            Func<T1, T2, T3, T4, T5> method, bool instanced, string parameterName1, string parameterName2,
            string parameterName3,
            string parameterName4)
        {
            AddCommand(command, description, method.Method, instanced, method.Target,
                new string[4] {parameterName1, parameterName2, parameterName3, parameterName4});
        }

        public static void AddCommand(string command, string description, bool instanced, Delegate method,
            params string[] parameterNames)
        {
            AddCommand(command, description, method.Method, instanced, method.Target, parameterNames);
        }

        #endregion
        
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
    }
}