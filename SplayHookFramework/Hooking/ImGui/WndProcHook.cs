using static SplayHookFramework.Misc.Native;

namespace SplayHookFramework.Hooking.ImGui;

public static class WndProcHook
{
    public static IntPtr OriginalWndProc = IntPtr.Zero;

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // 保持委托引用避免GC回收
    private static WndProcDelegate _ = WndProcImpl;

    public static IntPtr WndProcImpl(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
    {
        return CallWindowProc(OriginalWndProc, hWnd, uMsg, wParam, lParam);
    }
}