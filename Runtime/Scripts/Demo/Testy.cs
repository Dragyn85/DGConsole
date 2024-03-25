using System;
using System.Collections;
using DragynGames.Console;
using UnityEngine;

[ConsoleAvailable]
public class Testy : MonoBehaviour
{
    private TestyToo testyToo;
    
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        testyToo = new TestyToo();
    }

    [ConsoleAction("ds", "sdsa")]
    void DoStuff()
    {
        Debug.LogError("DOING STUFF!");
        Destroy(gameObject);
    }

}

public class TestyToo
{
    public TestyToo()
    {
        MethodHandler.RegisterObjectInstance(this);
    }

    [ConsoleAction]
    void DoSneakyStuff()
    {
        Debug.LogError("Doing SneakyStuff!");
    }
}
