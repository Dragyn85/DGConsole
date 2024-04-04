using System;
using System.Reflection;

namespace DragynGames.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ConsoleActionAttribute : Attribute
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string[] ParameterNames { get; set; }

        public ConsoleActionAttribute()
        {
            
        }

        public ConsoleActionAttribute(string command, string description, params string[] parameterNames)
        {
            Command = command;
            Description = description;
            ParameterNames = parameterNames;
        }
    }
}