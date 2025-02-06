using ImGuiNET;
using System.Runtime.InteropServices;
using static SplayHookFramework.Misc.Native;

namespace SplayHookFramework.Hooking.ImGui;

public static class WndProcHook
{
    public static IntPtr OriginalWndProc = IntPtr.Zero;

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("cimgui", EntryPoint = "ImGui_ImplWin32_WndProcHandler",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern long ImGui_ImplWin32_WndProcHandler(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static IntPtr WndProcImpl(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
    {
        // try
        // {
        //     var message = Enum.Parse<WindowMessage>(uMsg.ToString());
        //     switch (message)
        //     {
        //         case WindowMessage.WM_LBUTTONDOWN:
        //             Debug.WriteLine($"鼠标按下了 {wParam} X: {GetXlParam(lParam)} Y:{GetYlParam(lParam)}");
        //             break;
        //         case WindowMessage.WM_LBUTTONUP:
        //             Debug.WriteLine($"鼠标抬起了 {wParam} X: {GetXlParam(lParam)} Y:{GetYlParam(lParam)}");
        //             break;
        //         case WindowMessage.WM_INPUT:
        //             Debug.WriteLine($"窗口移动到 {wParam} X: {GetXlParam(lParam)} Y:{GetYlParam(lParam)}");
        //             break;
        //         case WindowMessage.WM_MOUSEMOVE:
        //             Debug.WriteLine($"鼠标移动到 {wParam} X: {GetXlParam(lParam)} Y:{GetYlParam(lParam)}");
        //             break;
        //         case WindowMessage.WM_KEYDOWN:
        //             Debug.WriteLine($"键盘按下了 {Enum.Parse<ConsoleKey>(wParam.ToString())} {lParam}");
        //             break;
        //         case WindowMessage.WM_KEYUP:
        //             Debug.WriteLine($"键盘抬起了 {Enum.Parse<ConsoleKey>(wParam.ToString())} {lParam}");
        //             break;
        //         case WindowMessage.WM_CHAR:
        //             Debug.WriteLine($"键盘输入 {Enum.Parse<ConsoleKey>(wParam.ToString())} {lParam}");
        //             break;
        //         case WindowMessage.WM_SETCURSOR:
        //             Debug.WriteLine($"鼠标聚焦 {wParam} {lParam}");
        //             break;
        //
        //         default:
        //             Debug.WriteError($"未知事件: {message} {wParam} {lParam}");
        //             break;
        //     }
        // }
        // catch (Exception e)
        // {
        //     Debug.WriteLine($"无法解析事件: {uMsg} {wParam} {lParam}");
        // }
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