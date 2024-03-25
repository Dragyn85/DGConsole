using System;
using System.Reflection;
using UnityEngine;

namespace DragynGames
{
    public class ConsoleMethodInfo
    {
        public readonly MethodInfo method;
        public readonly Type[] parameterTypes;
        public readonly string command;
        public readonly string signature;
        public readonly string[] parameters;
        
        private object instance = null;
        
        public  bool Instanced => instance != null && !instance.Equals(null);

        public ConsoleMethodInfo(MethodInfo method, Type[] parameterTypes,
            string command,
            string signature, string[] parameters)
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
    }
}