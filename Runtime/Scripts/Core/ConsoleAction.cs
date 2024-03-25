using System;
using System.Reflection;

namespace DragynGames.Console
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ConsoleActionAttribute : Attribute
    {
        private string m_command;
        private string m_description;
        private string[] m_parameterNames;

        public string Command
        {
            get { return m_command; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public string[] ParameterNames
        {
            get { return m_parameterNames; }
        }

        public ConsoleActionAttribute()
        {
            m_command = null;
            m_description = null;
        }

        public ConsoleActionAttribute(string command, string description, params string[] parameterNames)
        {
            m_command = command;
            m_description = description;
            m_parameterNames = parameterNames;
        }

        public void SetField(string Command, string Description, params string[] parameterNames)
        {
            m_command = Command;
            m_description = Description;
            m_parameterNames = parameterNames;
        }
    }
}