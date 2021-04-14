using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class CursorMovement : MonoBehaviour
{
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

    POINT pointInstance;

    [DllImport("UWPLib")]
    extern static string Move(int x, int y);

    [DllImport("UWPLib")]
    extern static string ClickUp();

    [DllImport("UWPLib")]
    extern static string ClickDown();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetCursorPos(int x, int y);


    [SerializeField] public Vector2Int CurrentCursorXY;
    [SerializeField] Vector2 DownCursorXY;
    [SerializeField] Vector2 PreviousCursorXY;

    public bool IsPointerDown;

    public void OnPointerDown(int x, int y)
    {
        SetCursorPos(x, y);
        ClickDown();
        IsPointerDown = true;
    }

    public void OnPointerUp()
    {
        ClickUp();
        IsPointerDown = false;
    }



    private void Start()
    {

    }

    [SerializeField] 
    Transform cameraTransform;

    private void Update()
    {
        //PreviousCursorXY = CurrentCursorXY;

        GetCursorPos(out pointInstance);
        CurrentCursorXY = new Vector2Int(pointInstance.X, pointInstance.Y);



        //Debug.DrawLine(cameraTransform.position, Input.mousePosition, Color.red);

        //foreach (var uddTexture in GameObject.FindObjectsOfType<uDesktopDuplication.Texture>())
        //{
        //    var result = uddTexture.RayCast(cameraTransform.position, Input.mousePosition - cameraTransform.position);
        //    if (result.hit)
        //    {
        //        //Debug.DrawLine(result.position, result.position + result.normal, Color.yellow);
        //        //Debug.Log("COORD: " + result.coords + ", DESKTOP: " + result.desktopCoord);
        //    }
        //}

        //if (IsPointerDown)
        //{

        //}
    }


    Vector2 CalculateMove(Vector2 a, Vector2 b)
    {
        return b - a;
    }

}
