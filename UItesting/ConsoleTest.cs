using DragynGames.Console;
using UnityEngine;

[ConsoleAvailable]
public class ConsoleTest : MonoBehaviour
{
    [ConsoleAction]
    private void DoStuff(int parameter)
    {
        Debug.Log("Doing stuff!" + parameter);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            MethodHandler.FindMethodsStartingAsync("DoSt");
        }
    }
}
