using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private UI ui;
    [SerializeField]
    private RectTransform joystickRect;
    [SerializeField]
    private RectTransform otherJoystickRect;
    [SerializeField]
    private Joystick otherJoystickScript;
    private Vector2 dragStart;

    public void OnPointerUp(PointerEventData eventData)
    {
        ui.JoystickReleased(joystickRect.anchoredPosition);
        joystickRect.anchoredPosition = Vector2.zero;
        otherJoystickRect.anchoredPosition = Vector2.zero;
        otherJoystickScript.enabled = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        otherJoystickScript.enabled = false;
        dragStart = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var displacement = (eventData.position - dragStart).magnitude;
        var newpos = (eventData.position - dragStart).normalized * Mathf.Min(displacement, 70f);
        joystickRect.anchoredPosition = newpos;
        otherJoystickRect.anchoredPosition = newpos;
    }
}
