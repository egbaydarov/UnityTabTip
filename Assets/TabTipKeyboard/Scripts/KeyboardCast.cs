using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using System;
using System.IO;

public class KeyboardCast : MonoBehaviour
{
    [SerializeField]
    Vector2 upperLeftSource = new Vector2(0, 713);

    [SerializeField]
    Vector2 screenSize = new Vector2(578, 342);

    [SerializeField]
    Vector2 bitmapSize = new Vector2(578, 342);

    [SerializeField]
    Vector2 upperLeftDestination = new Vector2(0, 0);

    [SerializeField]
    UnityEngine.UI.Image img;

    const uint D3DADAPTER_DEFAULT = 0x0;

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageData
    {
        public int usLength;
        public IntPtr lpbData;
    };

    [DllImport("DirectXScreenshotLib.dll", EntryPoint = "?Direct3D9TakeScreenshot@@YA?AUhex_data@@XZ", CallingConvention = CallingConvention.StdCall)]
    extern static ImageData TakeScreenshot();

    [DllImport("DirectXScreenshotLib.dll", EntryPoint = "?CleanMem@@YAXXZ", CallingConvention = CallingConvention.StdCall)]
    extern static void CleanImageData();

    [DllImport("DirectXScreenshotLib.dll", EntryPoint = "?ReleaseAdapter@@YAXXZ", CallingConvention = CallingConvention.StdCall)]
    extern static void ReleaseDirect3d9();

    [DllImport("DirectXScreenshotLib.dll", EntryPoint = "?Direct3D9Setup@@YAXI@Z", CallingConvention = CallingConvention.StdCall)]
    extern static void SetupDirect3d9(uint adapter);

    Texture2D tex;

    void Start()
    {
        Texture2D.allowThreadedTextureCreation = true;
        tex = new Texture2D((int)screenSize.x, (int)screenSize.y, TextureFormat.BGRA32, false);
        SetupDirect3d9(D3DADAPTER_DEFAULT);
    }

    private void OnApplicationQuit()
    {
        ReleaseDirect3d9();
    }


    void Update()
    {
        ImageData data = TakeScreenshot();

        if (data.usLength != 0)
        {
            tex.LoadRawTextureData(data.lpbData, data.usLength);
            tex.Apply(false, true);

            CleanImageData();

            img.sprite = Sprite.Create(tex, new Rect(0, 0, screenSize.x, screenSize.y), new Vector2(0, 0));
        }
        else
        {
            enabled = false;
            Debug.LogError("Direct3d9 no access.");
        }
    }

}
