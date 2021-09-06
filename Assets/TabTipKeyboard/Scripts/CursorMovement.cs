using System.Runtime.InteropServices;
using UnityEngine;

public class CursorMovement : MonoBehaviour
{
    public static object _lock = new object();

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

    public POINT pointInstance;

    [DllImport("UWPLib")]
    extern static string Move(int x, int y);

    [DllImport("UWPLib")]
    extern static string ClickUp();

    [DllImport("UWPLib")]
    extern static string ClickDown();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int x, int y);


    public Vector2Int CanvasCursorXY { get; set; }
    public Vector2Int RealCursorXY;

    public bool IsPointerDown;

    public void PointerDown(int x, int y)
    {
        ClickDown();
        IsPointerDown = true;
    }

    public void PointerEnter(Vector2Int point)
    {
    }

    public void PointerUp()
    {
        ClickUp();
        IsPointerDown = false;
    }

    public void PointerMove(Vector2Int mv)
    {
        Move(mv.x, mv.y);
    }

    private void Start()
    {

    }

    public void UpdateRealCursor()
    {
        RealCursorXY = new Vector2Int(pointInstance.X, pointInstance.Y);
    }

    [SerializeField]
    Transform cameraTransform;

    private void Update()
    {
        lock (_lock)
        {
            GetCursorPos(out pointInstance);
            //Debug.Log($"REAL: {RealCursorXY}\n Canvas: {CanvasCursorXY}");
            if (pointInstance.X != 0 || pointInstance.Y != 0)
                UpdateRealCursor();
        }
    }

}
