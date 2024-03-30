using UnityEngine;

namespace DragynGames.Commands.Demo
{
    public class DemoObjectInstanceCommand
    {
        [SerializeField] private CommandManager commandManager;
        /// <summary>
        /// You must register the object instance with the MethodHandler to be able to use ConsoleAction attributes.
        /// </summary>
        public DemoObjectInstanceCommand()
        {
            commandManager.RegisterObjectInstance(this);
        }

        // If you try using this command without registering the object instance the method wont run as.
        [ConsoleAction]
        void DoSneakyStuff()
        {
            Debug.LogError("Doing SneakyStuff!");
        }
    }
}