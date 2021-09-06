using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

public class MonitorToKeyboard : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    int yUpSplit;
    [SerializeField]
    int yDownSplit;
    [SerializeField]
    int xLeftSplit;
    [SerializeField]
    int xRightSplit;

    [SerializeField]
    List<Vector4> Caps = new List<Vector4>();

    UnityEvent OnSpaceUp = new UnityEvent();

    CursorMovement cursorData;
    TMP_Text TextTip;
    CopyTextureBuffer keyboardTexture;
    EnableDisableCalibrationTips tipsDisabler;

    string FileName = "/CalData.dat";

    public void RunCalibrate()
    {
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

    public void AddCap()
    {
        //TODO
    }

    void Start()
    {
        LoadCalibratedData();
        keyboardTexture.UpdateTexutreWithBorders(yUpSplit, yDownSplit, xLeftSplit, xRightSplit);

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

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 80, 175, 30), new GUIContent("Calibrate Keyboard Texture")))
            RunCalibrate();
        if (GUI.Button(new Rect(10, 115, 70, 30), new GUIContent("Add Cap")))
            AddCap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnSpaceUp.Invoke();
        }
    }

    void SaveCalibratedData()
    {
        BinaryFormatter bf = new BinaryFormatter();

        CalibrationData data = new CalibrationData(yUpSplit, xRightSplit, yDownSplit, xLeftSplit, Caps);

        using (FileStream stream = new FileStream(Application.persistentDataPath + FileName, FileMode.Open))
        {
            bf.Serialize(stream, data);
        }
        Debug.Log("Caibration data saved!");
    }

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

            Caps = data.getUnityVectors();
            yUpSplit = data.yUpSplit;
            xRightSplit = data.xRightSplit;
            yDownSplit = data.yDownSplit;
            xLeftSplit = data.xLeftSplit;
            Debug.Log("Calibration data loaded!");
        }
        else
            Debug.LogWarning("There is no pre-saved calibration data data!");
    }
}

[Serializable]
class CalibrationData
{
    public int yUpSplit;
    public int yDownSplit;
    public int xLeftSplit;
    public int xRightSplit;

    [SerializeField]
    public Vector4Serializsable[] Caps;

    public List<Vector4> getUnityVectors()
    {
        List<Vector4> vectors = new List<Vector4>();
        foreach (var i in Caps)
            vectors.Add(new Vector4(i.x, i.y, i.z, i.w));
        return vectors;
    }

    public CalibrationData(int yUpSplit, int xRightSplit, int yDownSplit, int xLeftSplit, List<Vector4> caps)
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
    }

    public CalibrationData()
    {
    }

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
