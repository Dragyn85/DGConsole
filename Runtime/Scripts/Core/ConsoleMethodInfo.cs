using System;
using System.Reflection;
using DragynGames.Console;
using UnityEngine;

namespace DragynGames
{
    internal class ConsoleMethodInfo
    {
        public readonly MethodInfo method;
        public readonly Type[] parameterTypes;
        public readonly string command;
        public readonly string signature;
        public readonly string[] parameters;
        public readonly string description;
        public readonly CommandType commandType;

        private object instance = null;

        public ConsoleMethodInfo(MethodInfo method,
            Type[] parameterTypes,
            string command,
            string signature,
            string[] parameters,
            object instance,
            CommandType commandType)
        {
            this.method = method;
            this.parameterTypes = parameterTypes;
            this.command = command;
            this.signature = signature;
            this.parameters = parameters;
        }

        
        public ConsoleMethodInfo(MethodInfo method, Type[] parameterTypes, object instance, string command,
            string signature, string[] parameters)
        {
            this.method = method;
            this.parameterTypes = parameterTypes;
            this.instance = instance;
            this.command = command;
            this.signature = signature;
            this.parameters = parameters;
            this.instance = instance;
        }

        public bool IsValid()
        {
            //if (!method.IsStatic && (instance == null || instance.Equals(null)))
            //{
            //    return false;
            //}


            return true;
        }

        public bool TryGetInstnace(out object instance)
        {
            instance = this.instance;
            return instance != null && !instance.Equals(null);
        }

        public void SetInstance(object instace)
        {
            this.instance = instace;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ConsoleMethodInfo))
                return false;
            ConsoleMethodInfo other = (ConsoleMethodInfo) obj;

            if (!command.Equals(other.command))
                return false;

            if (parameterTypes.Length != other.parameterTypes.Length)
                return false;

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (!parameterTypes[i].Equals(other.parameterTypes[i]))
                    return false;
            }

            return true;
        }
    }
}