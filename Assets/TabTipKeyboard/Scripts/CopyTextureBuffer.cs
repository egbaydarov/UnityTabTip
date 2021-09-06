using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class CopyTextureBuffer : MonoBehaviour
{
    [SerializeField] uDesktopDuplication.Texture uddTexture;
    [SerializeField] UnityEngine.UI.Image img;

    Texture2D texture_;
    Color32[] pixels_;
    GCHandle handle_;
    IntPtr ptr_ = IntPtr.Zero;

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

    void Start()
    {
        if (!uddTexture) return;

        uddTexture.monitor.useGetPixels = true;
        UpdateTexture();
    }

    int yDownSplit;
    int yUpSplit;
    int xLeftSplit;
    int xRightSplit;

    Rect GetKeyboardRectangle()
    {
        return new Rect(xLeftSplit, yUpSplit, xRightSplit - xLeftSplit, yDownSplit - yUpSplit);
    }

    public void UpdateTexutreWithBorders(int yUpSplit, int yDownSplit,int xLeftSplit, int xRightSplit)
    {
        this.yUpSplit = yUpSplit;
        this.yDownSplit = yDownSplit;
        this.xLeftSplit = xLeftSplit;
        this.xRightSplit = xRightSplit;
        UpdateTexture();
    }

    void OnDestroy()
    {
        if (ptr_ != IntPtr.Zero)
        {
            handle_.Free();
        }
    }


    void Update()
    {
        if (!uddTexture) return;

        if (uddTexture.monitor.width != texture_.width ||
            uddTexture.monitor.height != texture_.height)
        {
            UpdateTexture();
        }

        CopyTexture();
    }

    public Vector3 ImageVectorToDesktopPos(Vector3 ImageLocalPosition)
    {
        Vector2 size = this.GetComponent<Transform>().localScale;
        return new Vector3((xLeftSplit + xRightSplit) / 2.0f + ImageLocalPosition.x, (yDownSplit + yUpSplit) / 2.0f + ImageLocalPosition.y, 0);
    }

    void UpdateTexture()
    {
        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;

        // TextureFormat.BGRA32 should be set but it causes an error now.
        texture_ = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture_.filterMode = FilterMode.Bilinear;
        pixels_ = texture_.GetPixels32();
        handle_ = GCHandle.Alloc(pixels_, GCHandleType.Pinned);
        ptr_ = handle_.AddrOfPinnedObject();

        var ChildTransforms = gameObject.GetComponentsInChildren<RectTransform>();
        foreach(var transform in ChildTransforms)
        {
            transform.sizeDelta = new Vector2(xRightSplit - xLeftSplit, yDownSplit - yUpSplit);
        }

        img.sprite = Sprite.Create(texture_, GetKeyboardRectangle(), new Vector2(0, 0));
    }

    void CopyTexture()
    {
        var buffer = uddTexture.monitor.buffer;
        if (buffer == IntPtr.Zero) return;

        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;
        memcpy(ptr_, buffer, width * height * sizeof(Byte) * 4);

        texture_.SetPixels32(pixels_);
        texture_.Apply();
    }
}
