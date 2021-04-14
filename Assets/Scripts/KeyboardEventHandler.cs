using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardEventHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler //TODO
{
    bool IsHovered;
    bool IsPressed;

    CursorMovement cursor;

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerCurrentRaycast.worldPosition);
        IsPressed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(FindObjectOfType<EventSystem>() == null)
        {
            enabled = false;
            Debug.LogError("KeyboardEventHandeler: No Event System In scene (Add manually for keyboard gestures support)");
        }

        if (FindObjectOfType<BaseInputModule>() == null)
        {
            enabled = false;
            Debug.LogError("KeyboardEventHandeler: No Input Module In scene (Add manually for keyboard gestures support)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsHovered)
        {

        }
    }

    private void Awake()
    {
        cursor = GetComponent<CursorMovement>();
    }
}
