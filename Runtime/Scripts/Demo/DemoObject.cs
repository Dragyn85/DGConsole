using System.Collections;
using DragynGames.Commands;
using UnityEditor;
using UnityEngine;

namespace DragynGames.Commands.Demo
{
    public class DemoObject : MonoBehaviour
    {

        [SerializeField] private int _myInnateValue; 
        
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
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
        [ConsoleAction("AddToMe", "Adds two numbers to my innate number", "number1", "number2")]
        int AddNumbers(int a, int b)
        {
            return a + b + _myInnateValue;
        }

        public void SendLog()
        {
            Debug.Log("Sending log");
        }
        public void SendError()
        {
            Debug.LogError("Sending error");
        }
        public void SendWarning()
        {
            Debug.LogWarning("Sending warning");
        }
    }
}