using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ButtonHoldCheckerEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        // Debug.Log("Button is being held down.");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        // Debug.Log("Button released.");
    }
}
