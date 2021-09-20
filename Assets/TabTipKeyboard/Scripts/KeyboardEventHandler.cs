using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles unity UI events
/// </summary>
public class KeyboardEventHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
	/// Is pointer over keyboard flag
	/// </summary>
    bool IsHovered;

    /// <summary>
	/// Movement multiplication
	/// </summary>
    public const int MagicCoeff = 10;

    /// <summary>
	/// Texture buffer manager
	/// </summary>
    CopyTextureBuffer texture;

    /// <summary>
	/// Cursor manager
	/// </summary>
    CursorMovement cursor;

    /// <summary>
	/// Current event data
	/// </summary>
    PointerEventData frameEventData;

    /// <summary>
	/// Pointer down handler
	/// </summary>
	/// <param name="eventData">Current event data</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 DesktopPos = texture.ImageVectorToDesktopPos(transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition));
        cursor.PointerDown((int)DesktopPos.x, (int)DesktopPos.y);
    }

    /// <summary>
	/// Pointer over handler
	/// </summary>
	/// <param name="eventData">Current event data</param>
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

    /// <summary>
	/// Pointer enter handler
	/// </summary>
	/// <param name="eventData">current event data</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 DesktopPos = texture.ImageVectorToDesktopPos(transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition));
        IsHovered = true;
        frameEventData = eventData;
        cursor.PointerEnter(new Vector2Int((int)DesktopPos.x, (int)DesktopPos.y));
    }

    /// <summary>
	/// Handle pointer exit event
	/// </summary>
	/// <param name="eventData">Current event data</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
    }

    /// <summary>
	/// Handle pointer up event
	/// </summary>
	/// <param name="eventData">Current event data</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        cursor.PointerUp();
    }

    /// <summary>
	/// Setuo script
	/// </summary>
    private void Awake()
    {
        texture = FindObjectOfType<CopyTextureBuffer>();
        cursor = FindObjectOfType<CursorMovement>();
    }

    /// <summary>
	/// Validate scene setup
	/// </summary>
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

    /// <summary>
	/// Produce pinter over event each frame`
	/// </summary>
    void Update()
    {
        if (IsHovered)
        {
            if (frameEventData != null)
                OnPointerOver(frameEventData);
        }
    }

}
