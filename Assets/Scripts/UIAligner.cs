using UnityEngine;
using UnityEngine.UIElements;

public class UIAligner : MonoBehaviour
{
    public Camera pixelCam;
    public UIDocument uiDoc;

    void OnEnable()
    {
        var root = uiDoc.rootVisualElement;
        var rect = pixelCam.rect;

        root.style.position = Position.Absolute;
        root.style.left = Length.Percent(rect.x * 100);
        root.style.top = Length.Percent((1 - rect.y - rect.height) * 100);
        root.style.width = Length.Percent(rect.width * 100);
        root.style.height = Length.Percent(rect.height * 100);
    }
}
