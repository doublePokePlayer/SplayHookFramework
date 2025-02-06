using ImGuiNET;
using System.Runtime.InteropServices;
using static SplayHookFramework.Misc.Native;

namespace SplayHookFramework.Hooking.ImGui;

public static class WndProcHook
{
    /// <summary>
    /// 原始的窗口过程处理函数
    /// </summary>
    public static IntPtr OriginalWndProc = IntPtr.Zero;

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("cimgui", EntryPoint = "ImGui_ImplWin32_WndProcHandler",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern long ImGui_ImplWin32_WndProcHandler(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static IntPtr WndProcImpl(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
    {
        if ((GetAsyncKeyState((int)ConsoleKey.End) & 1) != 0) GlobalSettings.Running = false;

        if (!GlobalSettings.Running) return CallWindowProc(OriginalWndProc, hWnd, uMsg, wParam, lParam);

        if ((GetAsyncKeyState((int)ConsoleKey.Home) & 1) != 0)
        {
            ImGuiHookBase.Open = !ImGuiHookBase.Open;
            return 1;
        }

        if (ImGuiHookBase.Open && ImGui_ImplWin32_WndProcHandler(hWnd, uMsg, wParam, lParam) != 0)
        {
            ImGuiNET.ImGui.GetIO().MouseDrawCursor = ImGuiNET.ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);
            return 1;
        }

        if (ImGuiNET.ImGui.GetIO().WantCaptureMouse)
        {
            if (ImGuiNET.ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
                return 1;
            return 0;
        }

        return CallWindowProc(OriginalWndProc, hWnd, uMsg, wParam, lParam);
    }
}