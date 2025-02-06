using System.Runtime.InteropServices;

namespace SplayHookFramework.Misc;

public static class Native
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);


    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DisableThreadLibraryCalls(IntPtr hModule);


    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    #region 窗口过程

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrA")]
    private static extern IntPtr SetWindowLongPtrA(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IsWindowUnicode(hWnd)
            ? SetWindowLongPtrW(hWnd, nIndex, dwNewLong)
            : SetWindowLongPtrA(hWnd, nIndex, dwNewLong);
    }

    [DllImport("user32.dll")]
    public static extern IntPtr CallWindowProc(
        IntPtr lpPrevWndFunc, // 原始窗口过程指针
        IntPtr hWnd, // 窗口句柄
        uint msg, // 消息类型（如 WM_PAINT）
        IntPtr wParam, // 消息参数
        IntPtr lParam // 消息参数
    );

    #endregion

    #region 窗口消息

    /// <summary>
    /// 获取低16位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static short LowWord(IntPtr input)
    {
        return (short)input;
    }

    /// <summary>
    /// 获取高16位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static short HighWord(IntPtr input)
    {
        return (short)(input >> 16);
    }

    /// <summary>
    /// 从lParam解析X坐标
    /// </summary>
    /// <param name="lParam"></param>
    /// <returns></returns>
    public static short GetXlParam(IntPtr lParam)
    {
        return LowWord(lParam);
    }

    /// <summary>
    /// 从lParam解析Y坐标
    /// </summary>
    /// <param name="lParam"></param>
    /// <returns></returns>
    public static short GetYlParam(IntPtr lParam)
    {
        return HighWord(lParam);
    }

    #endregion

    public static IntPtr GetWindowLong(IntPtr hWnd, GWL nIndex)
    {
        return IsWindowUnicode(hWnd) ? GetWindowLongW(hWnd, nIndex) : GetWindowLongA(hWnd, nIndex);
    }

    public static IntPtr GetWindowLongA(IntPtr hWnd, GWL nIndex)
    {
        var is64Bit = Environment.Is64BitProcess;
        return is64Bit ? GetWindowLongPtr64(hWnd, (int)nIndex) : GetWindowLongPtr32(hWnd, (int)nIndex);
    }

    public static IntPtr GetWindowLongW(IntPtr hWnd, GWL nIndex)
    {
        var is64Bit = Environment.Is64BitProcess;
        return is64Bit ? GetWindowLongPtr64W(hWnd, (int)nIndex) : GetWindowLongPtr32W(hWnd, (int)nIndex);
    }

    public enum GWL
    {
        GWL_WNDPROC = -4,
        GWL_HINSTANCE = -6,
        GWL_HWNDPARENT = -8,
        GWL_STYLE = -16,
        GWL_EXSTYLE = -20,
        GWL_USERDATA = -21,
        GWL_ID = -12
    }

    [DllImport("user32.dll")]
    private static extern bool IsWindowUnicode(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongW")]
    private static extern IntPtr GetWindowLongPtr32W(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64W(IntPtr hWnd, int nIndex);
}