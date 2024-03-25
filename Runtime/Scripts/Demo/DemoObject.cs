using System.Collections;
using DragynGames.Console;
using UnityEditor;
using UnityEngine;

namespace DragynGames.Console.Demo
{
    public class DemoObject : MonoBehaviour
    {
        private DemoObjectInstanceCommand _demoObjectInstanceCommand;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            _demoObjectInstanceCommand = new DemoObjectInstanceCommand();
        }
        
        // STATIC METHOD
        // No further action is required to use this command.
        // Blank ConsoleAction attribute will use the method name as the command.
        [ConsoleAction]
        static void DoStaticStuff()
        {
            Debug.LogError("DOING STATIC STUFF!");
        }

        
        // NONE STATIC METHOD INSIDE A MONOBEHAVIOUR CLASS
        // MethodHandle will look for a gameobject of method holder type and register it.
        
        [ConsoleAction("DS", "Does stuff")]
        void DoStuff()
        {
            Debug.LogError("DOING STUFF!");
            Destroy(gameObject);
        }

        // METHOD WITH PARAMETERS AND RETURNTYPE
        // Define explanation and parameter names for the command if needed.
        // returned value will be logged to the console.
        [ConsoleAction("Add", "Adds two numbers and return value", "number1", "number2")]
        int AddNumbers(int a, int b)
        {
            return a + b;
        }
    }
}