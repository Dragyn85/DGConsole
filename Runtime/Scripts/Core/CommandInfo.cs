using System;
using System.Reflection;
using DragynGames.Commands;
using UnityEngine;

namespace DragynGames.Commands
{
    internal class CommandInfo
    {
        public readonly MethodInfo method;
        public readonly Type[] parameterTypes;
        public readonly string command;
        public readonly string signature;
        public readonly string[] parameters;
        public readonly string description;
        public readonly CommandType commandType;

        private object instance = null;

        public CommandInfo(MethodInfo method,
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
            this.instance = instance;
            this.commandType = commandType;
        }

        public bool IsValid()
        {
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
            if (obj == null || !(obj is CommandInfo))
                return false;
            CommandInfo other = (CommandInfo) obj;

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
        
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (command?.GetHashCode() ?? 0);
            hash = hash * 31 + parameterTypes.Length;
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                hash = hash * 31 + (parameterTypes[i]?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}