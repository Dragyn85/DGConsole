using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowSpawner : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset ConsoleScrollview;
    [SerializeField] private GameObject windowPrefab;

    private GameObject consoleWindow;

    private void Awake()
    {
        consoleWindow = Instantiate(windowPrefab);
        }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        var movableWindow = consoleWindow.GetComponent<ResizableMovableWindow>();
        VisualElement contentToAdd = ConsoleScrollview.Instantiate();
        contentToAdd.style.flexGrow = 1;
        contentToAdd.style.position = Position.Absolute;
        movableWindow.AddContent(contentToAdd);
    }
}