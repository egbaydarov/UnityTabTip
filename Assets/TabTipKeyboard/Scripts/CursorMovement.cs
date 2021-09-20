using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Control cursor events and cursor movement
/// </summary>
public class CursorMovement : MonoBehaviour
{
    /// <summary>
	/// Static lock object
	/// </summary>
    public static object _lock = new object();

    /// <summary>
	/// Camera transform
	/// </summary>
    [SerializeField]
    Transform cameraTransform;

    /// <summary>
	/// structure for coords of cursor
	/// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator System.Drawing.Point(POINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator POINT(System.Drawing.Point p)
        {
            return new POINT(p.X, p.Y);
        }
    }

    /// <summary>
	/// Ointer to stracture to store data given by GetCursorPos function
	/// </summary>
    public POINT pointInstance;

    /// <summary>
	/// Move cursor
	/// </summary>
	/// <param name="x">Delta x</param>
	/// <param name="y">Delta y</param>
	/// <returns>Message</returns>
    [DllImport("UWPLib")]
    extern static string Move(int x, int y);

    /// <summary>
	/// Make mouse click up programatcally
	/// </summary>
	/// <returns>Message</returns>
    [DllImport("UWPLib")]
    extern static string ClickUp();

    /// <summary>
	/// Make mouse click down programatcally
	/// </summary>
	/// <returns>Message</returns>
    [DllImport("UWPLib")]
    extern static string ClickDown();

    /// <summary>
	/// Get curent cursor position
	/// </summary>
	/// <param name="lpPoint">Protected pointer to structure to store values</param>
	/// <returns>Success status</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    /// <summary>
	/// Set cursor position
	/// </summary>
	/// <param name="x">X-axis coord</param>
	/// <param name="y">Y-axis coord</param>
	/// <returns>Success status</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int x, int y);

    /// <summary>
	/// Cursor coords on unity canvas
	/// </summary>
    public Vector2Int CanvasCursorXY { get; set; }

    /// <summary>
	/// Cursor coords on desktop
	/// </summary>
    public Vector2Int RealCursorXY;

    /// <summary>
	/// Pointer down flag
	/// </summary>
    public bool IsPointerDown;

    /// <summary>
	/// Pointer down handler
	/// </summary>
	/// <param name="x">X-axis coord</param>
	/// <param name="y">Y-axis coord</param>
    public void PointerDown(int x, int y)
    {
        ClickDown();
        IsPointerDown = true;
    }

    /// <summary>
	/// Pointer Enter handler
	/// </summary>
	/// <param name="point">Entry point</param>
    public void PointerEnter(Vector2Int point)
    {
    }

    /// <summary>
	/// Pointer Up Handler
	/// </summary>
    public void PointerUp()
    {
        ClickUp();
        IsPointerDown = false;
    }

    /// <summary>
	/// Pointer Move Handler
	/// </summary>
	/// <param name="mv">Delta vector</param>
    public void PointerMove(Vector2Int mv)
    {
        Move(mv.x, mv.y);
    }

    /// <summary>
	/// Update cursor position on screen
	/// </summary>
    public void UpdateRealCursor()
    {
        RealCursorXY = new Vector2Int(pointInstance.X, pointInstance.Y);
    }

    private void Update()
    {
        lock (_lock)
        {
            GetCursorPos(out pointInstance);
            if (pointInstance.X != 0 || pointInstance.Y != 0)
                UpdateRealCursor();
        }
    }

}
