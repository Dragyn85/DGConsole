using UnityEngine;
using UnityEngine.UIElements;

public class ResizableMovableWindow : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxmlAsset;
    [SerializeField] private StyleSheet styleSheet;
    [SerializeField] private float minWidth;
    [SerializeField] private float minHeight;

    private VisualElement window;
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        window = uxmlAsset.CloneTree().Q<VisualElement>("resizable-window");
        root.Q<GroupBox>("Background").Add(window);

        var header = root.Q<VisualElement>("window-header");
        var resizer = root.Q<VisualElement>("Resizer");
        var windowContent = root.Q<VisualElement>("window-content");
        

        window.styleSheets.Add(styleSheet);

        Vector2 originalWindowSize = Vector2.zero;
        Vector2 mouseDownPosition = Vector2.zero;
        Vector2 originalWindowPosition = Vector2.zero;

        resizer.RegisterCallback<MouseDownEvent>(evt =>
        {
            originalWindowSize = new Vector2(window.layout.width, window.layout.height);
            mouseDownPosition = evt.mousePosition;
            root.RegisterCallback<MouseMoveEvent>(OnResizerMouseMove);
        });

        header.RegisterCallback<MouseDownEvent>(evt =>
        {
            originalWindowPosition = window.transform.position;
            mouseDownPosition = evt.mousePosition;
            root.RegisterCallback<MouseMoveEvent>(OnHeaderMouseMove);
        });

        root.RegisterCallback<MouseUpEvent>(evt =>
        {
            root.UnregisterCallback<MouseMoveEvent>(OnResizerMouseMove);
            root.UnregisterCallback<MouseMoveEvent>(OnHeaderMouseMove);
        });

        void OnResizerMouseMove(MouseMoveEvent evt)
        {
            Vector2 delta = evt.mousePosition - mouseDownPosition;
            window.style.width = Mathf.Max(originalWindowSize.x + delta.x, minWidth);
            window.style.height = Mathf.Max(originalWindowSize.y + delta.y, minHeight);
        }

        void OnHeaderMouseMove(MouseMoveEvent evt)
        {
            Vector2 delta = evt.mousePosition - mouseDownPosition;
            window.transform.position = originalWindowPosition + delta;
        }
    }

    public void AddContent(VisualElement contentToAdd)
    {
        if (window == null)
        {
            return;
        }
        var scrollrect = new ScrollView();
        
        var contentWindow = window.Q<VisualElement>("window-content");
        scrollrect.style.flexGrow = 1;
        scrollrect.Add(contentToAdd);

        
        contentWindow.Add(scrollrect);
    }
    
    
}