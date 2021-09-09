using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class CopyTextureBuffer : MonoBehaviour
{
    [SerializeField] uDesktopDuplication.Texture uddTexture;
    [SerializeField] UnityEngine.UI.Image img;

    Texture2D texture_;
    List<Texture2D> _capsTextures = new List<Texture2D>();
    List<UnityEngine.UI.Image> _capsImages = new List<UnityEngine.UI.Image>();
    List<GameObject> _capsGOs = new List<GameObject>();
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

    Vector2 imgCenter => new Vector2((xRightSplit + xLeftSplit) / 2.0f, (yDownSplit + yUpSplit) / 2.0f);

    Rect GetKeyboardRectangle()
    {
        return new Rect(xLeftSplit, yUpSplit, xRightSplit - xLeftSplit, yDownSplit - yUpSplit);
    }

    internal void DestroyCaps()
    {
        for (int i = 0; i < _capsGOs.Count; ++i)
        {
            Destroy(_capsGOs[i]);
        }

        _capsGOs.Clear();
        _capsImages.Clear();
        _capsTextures.Clear();
    }

    public void UpdateTexutreWithBorders(int yUpSplit, int yDownSplit,int xLeftSplit, int xRightSplit)
    {
        this.yUpSplit = yUpSplit;
        this.yDownSplit = yDownSplit;
        this.xLeftSplit = xLeftSplit;
        this.xRightSplit = xRightSplit;
        UpdateTexture();
    }

    public void AddCap(Vector4 cap, Vector2Int colorCoords)
    {
        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;

        var yUp = cap.x;
        var yDown = cap.y;
        var xLeft = cap.z;
        var xRight = cap.w;
        var color = texture_.GetPixel(colorCoords.x, colorCoords.y);
        var rectangle = new Rect(xLeft, yUp, xRight - xLeft, yDown - yUp);
        var center = new Vector2((xRight + xLeft) / 2.0f, (yDown + yUp) / 2.0f) - imgCenter;
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels32(pixels);
        texture.Apply();

        var tempGo = new GameObject();
        UnityEngine.UI.Image capImage = tempGo.AddComponent<UnityEngine.UI.Image>();
        capImage.sprite = Sprite.Create(texture, rectangle, new Vector2(0, 0));
        tempGo.GetComponent<RectTransform>().SetParent(this.transform);
        tempGo.transform.localScale = img.gameObject.transform.localScale;
        tempGo.transform.localRotation = img.gameObject.transform.localRotation;
        var rect = tempGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(xRight - xLeft, yDown - yUp);
        tempGo.transform.localPosition = new Vector3(center.x, -center.y);
        tempGo.SetActive(true);

        _capsImages.Add(capImage);
        _capsGOs.Add(tempGo);

        _capsTextures.Add(texture);
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

        if(CopyTexture() && _capsGOs.Count == 0)
        {
            FindObjectOfType<MonitorToKeyboard>().LoadCaps();
        }
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

        //TextureFormat.BGRA32 should be set but it causes an error now.
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

    bool CopyTexture()
    {
        var buffer = uddTexture.monitor.buffer;
        if (buffer == IntPtr.Zero) return false;

        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;
        memcpy(ptr_, buffer, width * height * sizeof(Byte) * 4);

        texture_.SetPixels32(pixels_);
        texture_.Apply();

        return true;
    }

    public void LoadAllCaps(List<Vector4> caps, int[] yColor, int[] xColor)
    {
        for(int i = 0; i < caps.Count; ++i)
        {
            AddCap(caps[i], new Vector2Int(xColor[i], yColor[i]));
        }
    }
}
