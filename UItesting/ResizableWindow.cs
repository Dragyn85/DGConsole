using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResizableWindow : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxmlAsset;
    [SerializeField] private StyleSheet styleSheet;

    
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var window = uxmlAsset.CloneTree().Q<VisualElement>("resizable-window");
        root.Add(window);

        var resizer = window.Q<VisualElement>("window-resizer");
        var windowContent = window.Q<VisualElement>("window-content");

        windowContent.styleSheets.Add(styleSheet);

        Vector2 originalWindowSize = Vector2.zero;
        Vector2 mouseDownPosition = Vector2.zero;

        resizer.RegisterCallback<MouseDownEvent>(evt =>
        {
            originalWindowSize = new Vector2(window.layout.width, window.layout.height);
            mouseDownPosition = evt.mousePosition;
            root.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        });

        root.RegisterCallback<MouseUpEvent>(evt =>
        {
            root.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        });

        void OnMouseMove(MouseMoveEvent evt)
        {
            Vector2 delta = evt.mousePosition - mouseDownPosition;
            window.style.width = Mathf.Max(originalWindowSize.x + delta.x, resizer.layout.width);
            window.style.height = Mathf.Max(originalWindowSize.y + delta.y, resizer.layout.height);
        }
    }

}
