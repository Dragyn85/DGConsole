using System;

namespace DragynGames.Console
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ConsoleAction : Attribute {
        public string Name { get; private set; }
        public ConsoleAction() {
            Name = null;
        }
        public ConsoleAction(string name) {
            Name = name;
        }
    }
}