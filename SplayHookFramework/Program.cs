using SplayHookFramework.Hooking.ImGui;
using SplayHookFramework.Misc;
using System.Runtime.InteropServices;

namespace SplayHookFramework;

public class Program
{
    [UnmanagedCallersOnly(EntryPoint = "DllMain")]
    public static bool DllMain(IntPtr hModule, uint reason, IntPtr _)
    {
        if (reason == 1)
        {
            Native.DisableThreadLibraryCalls(hModule);

            GlobalSettings.DllInstance = hModule;
            if (Native.AllocConsole()) GlobalSettings.UseConsole = true;

            GlobalSettings.Thread = Native.CreateThread(
                0,
                0,
                Marshal.GetFunctionPointerForDelegate(new StartDelegate(Start)),
                IntPtr.Zero,
                0,
                ref GlobalSettings.ThreadId);
        }

        return true;
    }

    private delegate int StartDelegate();

    private static int Start()
    {
        var dx11Hook = new ImGuiDx11Hook();
        dx11Hook.Initial();

        while (GlobalSettings.Running) Thread.Sleep(100);

        dx11Hook.Release();
        Native.CloseHandle(GlobalSettings.Thread);
        Native.FreeLibraryAndExitThread(GlobalSettings.DllInstance, 0);
        return 0;
    }
}