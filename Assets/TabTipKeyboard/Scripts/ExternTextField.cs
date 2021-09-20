using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using TMPro;
using UnityEngine;

/// <summary>
/// Control keyboard text fiels widget
/// </summary>
public class ExternTextField : MonoBehaviour
{
    /// <summary>
	/// Text data retrieved from shared memory
	/// </summary>
    [SerializeField]
    public string ExternTextFieldData = "";

    /// <summary>
	/// Extern text field process
	/// </summary>
    System.Diagnostics.Process TFProc;

    /// <summary>
	/// Keyboard process
	/// </summary>
    System.Diagnostics.Process TabTipProc;

    /// <summary>
	/// Unity scene input field
	/// </summary>
    [SerializeField]
    private TMP_InputField _inputField;

    /// <summary>
	/// Keuboard inputed data max buffer size
	/// </summary>
    [SerializeField]
    const int MMF_MAX_SIZE = 1024;
    /// <summary>
	/// Keuboard inputed data view buffer size
	/// </summary>
    [SerializeField]
    const int MMF_VIEW_SIZE = 1024;

    /// <summary>
	/// Shared memmory buffer
	/// </summary>
    byte[] buffer = new byte[MMF_VIEW_SIZE];

    /// <summary>
	/// MMVS
	/// </summary>
    MemoryMappedViewStream TextFieldDataStream = null;

    /// <summary>
	/// IShared Memmory reachable flag. Indicate about state of configured MMF
	/// </summary>
    public static bool IsSharedMomeryReachable = true;

    /// <summary>
	/// GUI setup
	/// </summary>
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 45, 110, 30), new GUIContent("Show Keyboard")))
            ShowKeyboard();
    }

    /// <summary>
	/// Auto restart widget
	/// </summary>
    private void Update()
    {
        if (TFProc == null || TFProc.HasExited)
        {
            RestartApp();
        }
        TextReader textReader = new StreamReader(TextFieldDataStream);
        ExternTextFieldData = textReader.ReadLine();
        _inputField.text = ExternTextFieldData;
        TextFieldDataStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
	/// Setup shared memory for text input data
	/// </summary>
    public void SharedMemorySetup()
    {
        using (var mmf = MemoryMappedFile.CreateOrOpen("TextField_Widget", MMF_MAX_SIZE, MemoryMappedFileAccess.ReadWrite))
        using (var mmvStream = mmf.CreateViewStream(0, MMF_VIEW_SIZE))
        {
            TextFieldDataStream = mmvStream;
            while (IsSharedMomeryReachable)
            {
                Thread.Sleep(100);
            }
        }
    }

    /// <summary>
	/// Script setup
	/// </summary>
    private void Start()
    {
        if(_inputField == null)
        {
            Debug.LogError("InputField property didn't set up. You should do that in inspector. Disabling.");
            enabled = false;
            return;
        }

        Thread textUpdate = new Thread(new ThreadStart(SharedMemorySetup));
        textUpdate.IsBackground = true;
        textUpdate.Start();

        RestartApp();
        ShowKeyboard();
    }

    /// <summary>
	/// Disables widget and keyboard
	/// </summary>
    private void OnApplicationQuit()
    {
        TFProc?.Kill();
        IsSharedMomeryReachable = false;
        //closeOnscreenKeyboard(); //TODO FIX COM EXCEPTION
    }

    /// <summary>
	/// Restart text field widget process
	/// </summary>
    public void RestartApp()
    {
        System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("TextFieldWidget");
        if (pname.Length == 0)
        {
            TFProc = System.Diagnostics.Process.Start(Application.dataPath + "\\StreamingAssets\\TabTipKeyboard\\TextFieldWidget.exe");
        }

        enabled = true;
    }

    /// <summary>
	/// Shows Windows Tab Tip Keyboard
	/// </summary>
    public void ShowKeyboard()
    {
        System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("tabtip");
        if (pname.Length == 0)
        {
            string tabtipId = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\microsoft shared\ink\tabtip.exe";
            TabTipProc = System.Diagnostics.Process.Start(tabtipId);
        }
        else
        {
            TabTipProc = pname[0];
        }
        if (!IsKeyboardVisible())
        {
            var uiHostNoLaunch = new UIHostNoLaunch();
            var tipInvocation = (ITipInvocation)uiHostNoLaunch;
            tipInvocation.Toggle(GetDesktopWindow());
            Marshal.ReleaseComObject(uiHostNoLaunch);
        }
    }


    /// <summary>
	/// Com registred class for UI control
	/// </summary>
    #region Show keyboard
    [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
    class UIHostNoLaunch
    {
    }

    /// <summary>
	/// Com registred interface for ui control class
	/// </summary>
    [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITipInvocation
    {
        void Toggle(IntPtr hwnd);
    }

    /// <summary>
	/// Get desktop window handle
	/// </summary>
	/// <returns>Windows handle pointer</returns>
    [DllImport("user32.dll", SetLastError = false)]
    static extern IntPtr GetDesktopWindow();
    #endregion

    #region check is keyboard visible
    /// <summary>
    /// The window is initially visible. See http://msdn.microsoft.com/en-gb/library/windows/desktop/ms632600(v=vs.85).aspx.
    /// </summary>
    public const UInt32 WS_VISIBLE = 0X94000000;
    /// <summary>
    /// Specifies we wish to retrieve window styles.
    /// </summary>
    public const int GWL_STYLE = -16;

    /// <summary>
	/// Get window by parameters
	/// </summary>
	/// <param name="sClassName">Class name</param>
	/// <param name="sAppName">App name</param>
	/// <returns>Windows pointer</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(String sClassName, String sAppName);

    /// <summary>
	/// Get window property
	/// </summary>
	/// <param name="hWnd"><Window pointer/param>
	/// <param name="nIndex">Property index</param>
	/// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);


    /// <summary>
    /// Checks to see if the virtual keyboard is visible.
    /// </summary>
    /// <returns>True if visible.</returns>
    public static bool IsKeyboardVisible()
    {
        IntPtr keyboardHandle = GetKeyboardWindowHandle();

        bool visible = false;

        if (keyboardHandle != IntPtr.Zero)
        {
            UInt32 style = GetWindowLong(keyboardHandle, GWL_STYLE);
            visible = (style == WS_VISIBLE);
        }

        return visible;
    }

    /// <summary>
	/// Returns keyboard window handle
	/// </summary>
	/// <returns></returns>
    public static IntPtr GetKeyboardWindowHandle()
    {
        return FindWindow("IPTip_Main_Window", null);
    }
    #endregion

    #region hide keyboard

    /// <summary>
	/// Send event message WinApi function
	/// </summary>
	/// <param name="hWnd"><Window handler/param>
	/// <param name="Msg">Message</param>
	/// <param name="wParam">w param</param>
	/// <param name="lParam">l param</param>
	/// <returns></returns>
    [DllImport("user32.dll")]
    public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);


    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_CLOSE = 0xF060;

    /// <summary>
	/// Closes keyboard (Deprecated)
	/// </summary>
    private void closeOnscreenKeyboard()
    {
        // retrieve the handler of the window  
        int iHandle = (int)FindWindow("IPTIP_Main_Window", "");
        if (iHandle > 0)
        {
            // close the window using API        
            SendMessage(iHandle, WM_SYSCOMMAND, SC_CLOSE, 0);
        }
    }
    #endregion


}
