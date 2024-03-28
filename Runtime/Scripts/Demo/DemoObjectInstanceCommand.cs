using UnityEngine;

namespace DragynGames.Commands.Demo
{
    public class DemoObjectInstanceCommand
    {
        /// <summary>
        /// You must register the object instance with the MethodHandler to be able to use ConsoleAction attributes.
        /// </summary>
        public DemoObjectInstanceCommand()
        {
            CommandManager.RegisterObjectInstance(this);
        }

        // If you try using this command without registering the object instance the method wont run as.
        [ConsoleAction]
        void DoSneakyStuff()
        {
            Debug.LogError("Doing SneakyStuff!");
        }
    }
}