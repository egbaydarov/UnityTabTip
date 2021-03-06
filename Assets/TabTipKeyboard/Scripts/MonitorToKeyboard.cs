using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

/// <summary>
/// Casts part of keyboard from desktop to unity
/// </summary>
public class MonitorToKeyboard : MonoBehaviour
{
    /// <summary>
	/// Y-axis top most coordinate of keyboard
	/// </summary>
    [SerializeField]
    int yUpSplit;
    /// <summary>
	/// Y-axis bottom most coordinate of keyboard
	/// </summary>
    [SerializeField]
    int yDownSplit;

    /// <summary>
	/// X-axis left most coordinate of keyboard
	/// </summary>
    [SerializeField]
    int xLeftSplit;

    /// <summary>
	/// X-axis right most coordinate of keyboard
	/// </summary>
    [SerializeField]
    int xRightSplit;

    /// <summary>
	/// Caps array to serialize
	/// </summary>
    [SerializeField]
    List<Vector4> Caps = new List<Vector4>();

    /// <summary>
	/// X color coordinates to serialize
	/// </summary>
    [SerializeField]
    int[] xColor = new int[1000];

    /// <summary>
	/// y-color coordinates to serialize
	/// </summary>
    [SerializeField]
    int[] yColor = new int[1000];

    /// <summary>
	/// Space up event
	/// </summary>
    UnityEvent OnSpaceUp = new UnityEvent();

    /// <summary>
	/// Cursor data manager
	/// </summary>
    CursorMovement cursorData;

    /// <summary>
	/// Text tip
	/// </summary>
    TMP_Text TextTip;

    /// <summary>
	/// Keyboard texture
	/// </summary>
    CopyTextureBuffer keyboardTexture;

    /// <summary>
	/// Disable/enable tips controller
	/// </summary>
    EnableDisableCalibrationTips tipsDisabler;

    /// <summary>
	/// Calibration data store file
	/// </summary>
    const string FileName = "/CalData.dat";

    /// <summary>
	/// Run calibration script
	/// </summary>
    public void RunCalibrate()
    {
        Caps.Clear();
        keyboardTexture.DestroyCaps();
        tipsDisabler.EnableDisableTips(true);

        int pointsCounter = 0;
        var vals = new Vector2Int[4];
        var tips = new string[]
        {
            "Put mouse pointer to upper border of Tab Tip Windows Keyboard then press Space button",
            "Put mouse pointer to down border of Tab Tip Windows Keyboard then press Space button",
            "Put mouse pointer to left border of Tab Tip Windows Keyboard then press Space button",
            "Put mouse pointer to right border of Tab Tip Windows Keyboard then press Space button",
            "Successful!"
        };
        TextTip.text = tips[pointsCounter];

        OnSpaceUp.RemoveAllListeners();
        OnSpaceUp.AddListener(() =>
        {
            vals[pointsCounter++] = new Vector2Int(cursorData.pointInstance.X, cursorData.pointInstance.Y);
            TextTip.text = tips[pointsCounter];


            if (pointsCounter == 4)
            {
                yUpSplit = vals[0].y;
                yDownSplit = vals[1].y;
                xLeftSplit = vals[2].x;
                xRightSplit = vals[3].x;

                keyboardTexture.UpdateTexutreWithBorders(yUpSplit, yDownSplit, xLeftSplit, xRightSplit);
                tipsDisabler.EnableDisableTips(false);
                OnSpaceUp.RemoveAllListeners();
                SaveCalibratedData();
            }
        });
    }

    /// <summary>
	/// Run add cap script
	/// </summary>
    public void AddCap()
    {
        tipsDisabler.EnableDisableTips(true);

        int pointsCounter = 0;
        var vals = new Vector2Int[5];
        var tips = new string[]
        {
            "Put mouse pointer to upper border of cap then press Space button",
            "Put mouse pointer to down border of cap then press Space button",
            "Put mouse pointer to left border of cap then press Space button",
            "Put mouse pointer to right border of cap then press Space button",
            "Put mouse pointer to pixel with cap color then press Space button",
            "Successful!"
        };
        TextTip.text = tips[pointsCounter];
        TextTip.text = tips[pointsCounter];

        OnSpaceUp.RemoveAllListeners();
        OnSpaceUp.AddListener(() =>
        {
            vals[pointsCounter++] = new Vector2Int(cursorData.pointInstance.X, cursorData.pointInstance.Y);
            TextTip.text = tips[pointsCounter];

            if (pointsCounter == 5)
            {
                Vector4 vec;
                vec.x = vals[0].y;
                vec.y = vals[1].y;
                vec.z = vals[2].x;
                vec.w = vals[3].x;
                xColor[Caps.Count] = vals[4].x;
                yColor[Caps.Count] = vals[4].y;
                Caps.Add(vec);

                keyboardTexture.AddCap(vec, new Vector2Int(vals[4].x, vals[4].y));
                tipsDisabler.EnableDisableTips(false);
                OnSpaceUp.RemoveAllListeners();
                SaveCalibratedData();
            }
        });
    }

    void Start()
    {
        LoadCalibratedData();
        keyboardTexture.UpdateTexutreWithBorders(yUpSplit, yDownSplit, xLeftSplit, xRightSplit);
    }

    /// <summary>
	/// Load caps
	/// </summary>
    public void LoadCaps()
    {
        keyboardTexture.LoadAllCaps(Caps, yColor, xColor);
    }

    private void Awake()
    {
        cursorData = FindObjectOfType<CursorMovement>();
        keyboardTexture = FindObjectOfType<CopyTextureBuffer>();
        tipsDisabler = FindObjectOfType<EnableDisableCalibrationTips>();
        var TempGO = GameObject.Find("TipTabTipUnity");
        if (TempGO == null)
        {
            enabled = false;
            Debug.Log("[MonitorToKeyboard] Can't find TipTabTibkeyboard GO.");
        }
        TextTip = TempGO.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
	/// GUI setup
	/// </summary>
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 80, 175, 30), new GUIContent("Calibrate Keyboard Texture")))
            RunCalibrate();
        if (GUI.Button(new Rect(10, 115, 70, 30), new GUIContent("Add Cap")))
            AddCap();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnSpaceUp.Invoke();
        }
    }

    /// <summary>
	/// save calibration data to disk
	/// </summary>
    void SaveCalibratedData()
    {
        BinaryFormatter bf = new BinaryFormatter();

        CalibrationData data = new CalibrationData(yUpSplit, xRightSplit, yDownSplit, xLeftSplit, Caps, xColor, yColor);

        using (FileStream stream = new FileStream(Application.persistentDataPath + FileName, FileMode.OpenOrCreate))
        {
            bf.Serialize(stream, data);
        }
        Debug.Log("Caibration data saved!");
    }

    /// <summary>
	/// load calibration data from disk
	/// </summary>
    void LoadCalibratedData()
    {

        if (File.Exists(Application.persistentDataPath + FileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            CalibrationData data;

            using (FileStream stream = new FileStream(Application.persistentDataPath + FileName, FileMode.Open))
            {
                data = (CalibrationData)bf.Deserialize(stream);
            }

            yUpSplit = data.yUpSplit;
            xRightSplit = data.xRightSplit;
            yDownSplit = data.yDownSplit;
            xLeftSplit = data.xLeftSplit;
            xColor = data.xColor;
            yColor = data.yColor;
            Caps = data.getUnityVectors();
            Debug.Log("Calibration data loaded!");
        }
        else
            Debug.LogWarning("There is no pre-saved calibration data data!");
    }
}

/// <summary>
/// Calibration data and caps model
/// </summary>
[Serializable]
class CalibrationData
{
    public int yUpSplit;
    public int yDownSplit;
    public int xLeftSplit;
    public int xRightSplit;

    [SerializeField]
    public Vector4Serializsable[] Caps;
    public int[] xColor;
    public int[] yColor;

    /// <summary>
	/// Generate from serializeble vectors unity vectors
	/// </summary>
	/// <returns>Unity vetors</returns>
    public List<Vector4> getUnityVectors()
    {
        List<Vector4> vectors = new List<Vector4>();
        foreach (var i in Caps)
            vectors.Add(new Vector4(i.x, i.y, i.z, i.w));
        return vectors;
    }

    public CalibrationData(int yUpSplit, int xRightSplit, int yDownSplit, int xLeftSplit, List<Vector4> caps, int[] yColor, int[] xColor)
    {
        this.yUpSplit = yUpSplit;
        this.yDownSplit = yDownSplit;
        this.xLeftSplit = xLeftSplit;
        this.xRightSplit = xRightSplit;
        Caps = new Vector4Serializsable[caps.Count];
        for (int i = 0; i < Caps.Length; ++i)
        {
            Caps[i] = new Vector4Serializsable(caps[i]);
        }
        this.xColor = xColor;
        this.yColor = yColor;
    }

    public CalibrationData()
    {
    }

    /// <summary>
	/// Serializable Vector4 implementation
	/// </summary>
    [Serializable]
    public class Vector4Serializsable
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public Vector4Serializsable()
        {
        }

        public Vector4Serializsable(Vector4 cap)
        {
            this.x = (int)cap.x;
            this.y = (int)cap.y;
            this.z = (int)cap.z;
            this.w = (int)cap.w;
        }
    }
}
