using System;
using System.Linq;
using System.Reflection;

namespace DragynGames.Commands
{
    internal class CommandDefinitionData
    {
        public readonly string Command;
        public readonly string[] Parameters;
        public readonly string Description;
        public readonly MethodInfo MethodInfo;
        public readonly CommandType CommandType;

        public object instance;
        
        private readonly ConsoleActionAttribute _attribute;

        internal CommandDefinitionData(ConsoleActionAttribute attribute, MethodInfo methodInfo, CommandType commandType
            ,ManualCommandCreationInfo manualCommandCreationInfo = new ManualCommandCreationInfo())
        {
            CommandType = commandType;


            if (attribute == null)
            {
                Command = string.IsNullOrEmpty(manualCommandCreationInfo.commandName)?methodInfo.Name : manualCommandCreationInfo.commandName;
                Description = manualCommandCreationInfo.description;
                Parameters = methodInfo.GetParameters().Select(t => t.Name).ToArray();
            }
            else
            {
                Command = string.IsNullOrEmpty(attribute.Command) ? methodInfo.Name : attribute.Command;
                Description = string.IsNullOrEmpty(attribute.Description) ? manualCommandCreationInfo.description : attribute.Description;
                Parameters = attribute.ParameterNames is {Length: > 0}
                    ? attribute.ParameterNames
                    : methodInfo.GetParameters().Select(t => t.Name).ToArray();
            }

            instance = manualCommandCreationInfo.instance;
            MethodInfo = methodInfo;
            _attribute = attribute;
        }
    }
}