using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardEventHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    bool IsHovered;
    public const int MagicCoeff = 10;

    CopyTextureBuffer texture;
    CursorMovement cursor;
    PointerEventData frameEventData;

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 DesktopPos = texture.ImageVectorToDesktopPos(transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition));
        cursor.PointerDown((int)DesktopPos.x, (int)DesktopPos.y);
    }

    public void OnPointerOver(PointerEventData eventData)
    {
        Vector3 DesktopPos = texture.ImageVectorToDesktopPos(transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition));
        //Debug.Log(DesktopPos);
        
        cursor.CanvasCursorXY = new Vector2Int((int)DesktopPos.x, (int)DesktopPos.y);
        Vector2Int mv = cursor.CanvasCursorXY - cursor.RealCursorXY;
        //CursorMovement.SetCursorPos(cursor.CanvasCursorXY.x, cursor.CanvasCursorXY.y);
        lock(CursorMovement._lock)
        {
                cursor.PointerMove(mv / MagicCoeff);
        }
        //Debug.Log($"MOVE: {mv}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 DesktopPos = texture.ImageVectorToDesktopPos(transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition));
        IsHovered = true;
        frameEventData = eventData;
        cursor.PointerEnter(new Vector2Int((int)DesktopPos.x, (int)DesktopPos.y));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        cursor.PointerUp();
    }


    private void Awake()
    {
        texture = FindObjectOfType<CopyTextureBuffer>();
        cursor = FindObjectOfType<CursorMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<EventSystem>() == null)
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
        if (IsHovered)
        {
            if (frameEventData != null)
                OnPointerOver(frameEventData);
        }
    }

}
