using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// Copy memory buffer with desktop image into unity texture GameObject
/// </summary>
public class CopyTextureBuffer : MonoBehaviour
{
    /// <summary>
	/// udd Api texture
	/// </summary>
    [SerializeField] uDesktopDuplication.Texture uddTexture;

    /// <summary>
	/// Image where keyboard will be shown
	/// </summary>
    [SerializeField] UnityEngine.UI.Image img;

    /// <summary>
	/// Store desktop image data
	/// </summary>
    Texture2D texture_;

    /// <summary>
	/// List of texture for hiding unneccesary parts of keyboard
	/// </summary>
    List<Texture2D> _capsTextures = new List<Texture2D>();

    /// <summary>
	/// List of images which hide unneccessary parts of keyboard
	/// </summary>
    List<UnityEngine.UI.Image> _capsImages = new List<UnityEngine.UI.Image>();

    /// <summary>
	/// List of GamObjects wchich hide unneccessary parts of keyboard
	/// </summary>
    List<GameObject> _capsGOs = new List<GameObject>();

    /// <summary>
	/// Arrray of pixels of desktop image
	/// </summary>
    Color32[] pixels_;

    /// <summary>
	/// Pointer to desktop image buffer
	/// </summary>
    GCHandle handle_;

    /// <summary>
	/// Zero pointer
	/// </summary>
    IntPtr ptr_ = IntPtr.Zero;

    /// <summary>
	/// Native "c" mem copy function
	/// </summary>
	/// <param name="dest">Pointer to destination buffre</param>
	/// <param name="src">Pointre to source buffer</param>
	/// <param name="count">Buffer length</param>
	/// <returns>Pointer to buffer</returns>
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

    /// <summary>
	/// Start unity MonoBehavior
	/// </summary>
    void Start()
    {
        if (!uddTexture) return;

        uddTexture.monitor.useGetPixels = true;
        UpdateTexture();
    }

    /// <summary>
	/// Y-axis bottom most ccord of keyboard rectangle
	/// </summary>
    int yDownSplit;
    /// <summary>
	/// Y-axis top most ccord of keyboard rectangle
	/// </summary>
    int yUpSplit;
    /// <summary>
	/// X-axis left most ccord of keyboard rectangle
	/// </summary>
    int xLeftSplit;
    /// <summary>
	/// X-axis right most ccord of keyboard rectangle
	/// </summary>
    int xRightSplit;

    /// <summary>
	/// Coords of center of keyboard image
	/// </summary>
    Vector2 imgCenter => new Vector2((xRightSplit + xLeftSplit) / 2.0f, (yDownSplit + yUpSplit) / 2.0f);

    /// <summary>
	/// Get Rectangle object defining keyboard
	/// </summary>
	/// <returns>Rectangle of last calibrated keyboard</returns>
    Rect GetKeyboardRectangle()
    {
        return new Rect(xLeftSplit, yUpSplit, xRightSplit - xLeftSplit, yDownSplit - yUpSplit);
    }

    /// <summary>
	/// Remove all saved caps
	/// </summary>
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

    /// <summary>
	/// Update texture according to given borders
	/// </summary>
	/// <param name="yUpSplit">Upper coord by y axis</param>
	/// <param name="yDownSplit">Lower coord by y axis</param>
	/// <param name="xLeftSplit">Left most coord by x axis</param>
	/// <param name="xRightSplit">Right most coord by x axis</param>
    public void UpdateTexutreWithBorders(int yUpSplit, int yDownSplit,int xLeftSplit, int xRightSplit)
    {
        this.yUpSplit = yUpSplit;
        this.yDownSplit = yDownSplit;
        this.xLeftSplit = xLeftSplit;
        this.xRightSplit = xRightSplit;
        UpdateTexture();
    }

    /// <summary>
	/// Adding cap by given coord of borders and coords of pixel with color
	/// </summary>
	/// <param name="cap">Coords of borders</param>
	/// <param name="colorCoords">Coords of pixel with color</param>
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

    /// <summary>
	/// Free handler of desktop image buffer
	/// </summary>
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

    /// <summary>
	/// Transform postion on image to position on desktop
	/// </summary>
	/// <param name="ImageLocalPosition">Local position on image</param>
	/// <returns>Desktop position</returns>
    public Vector3 ImageVectorToDesktopPos(Vector3 ImageLocalPosition)
    {
        Vector2 size = this.GetComponent<Transform>().localScale;
        return new Vector3((xLeftSplit + xRightSplit) / 2.0f + ImageLocalPosition.x, (yDownSplit + yUpSplit) / 2.0f + ImageLocalPosition.y, 0);
    }

    /// <summary>
	/// Updates texture size
	/// </summary>
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

    /// <summary>
	/// Copy buffer of desktop image into texture buffer
	/// </summary>
	/// <returns>Success status</returns>
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

    /// <summary>
	/// Load all caps into scene
	/// </summary>
	/// <param name="caps">Caps borders</param>
	/// <param name="yColor">Y-axis coords of caps colors</param>
	/// <param name="xColor">X-axis coords of caps colors</param>
    public void LoadAllCaps(List<Vector4> caps, int[] yColor, int[] xColor)
    {
        for(int i = 0; i < caps.Count; ++i)
        {
            AddCap(caps[i], new Vector2Int(xColor[i], yColor[i]));
        }
    }
}
