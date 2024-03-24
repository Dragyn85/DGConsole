using System;
using System.Reflection;

namespace DragynGames
{
    public class ConsoleMethodInfo
    {
        public readonly MethodInfo method;
        public readonly Type[] parameterTypes;
        public readonly object instance;
        public readonly bool isInstancedMethod;
        public readonly string command;
        public readonly string signature;
        public readonly string[] parameters;

        public ConsoleMethodInfo(MethodInfo method, Type[] parameterTypes, object instance, string command,
            string signature,bool isInstancedMethod, string[] parameters)
        {
            this.method = method;
            this.parameterTypes = parameterTypes;
            this.instance = instance;
            this.command = command;
            this.signature = signature;
            this.parameters = parameters;
            this.isInstancedMethod = isInstancedMethod;
        }
        public ConsoleMethodInfo(MethodInfo method, Type[] parameterTypes, bool instance, string command,
            string signature, string[] parameters)
        {
            this.method = method;
            this.parameterTypes = parameterTypes;
            this.instance = instance;
            this.command = command;
            this.signature = signature;
            this.parameters = parameters;
            this.isInstancedMethod = false;
        }
        public bool IsValid()
        {
            if (!method.IsStatic && (instance == null || instance.Equals(null)))
                return false;

            return true;
        }
    }
}
