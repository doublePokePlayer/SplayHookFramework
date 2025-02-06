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

            Task.Run(Start);
        }

        return true;
    }

    private static async void Start()
    {
        var dx11Hook = new ImGuiDx11Hook();
        dx11Hook.Initial();

        while (GlobalSettings.Running) await Task.Delay(100);

        dx11Hook.Release();
        // TODO: 进程会崩溃 原因未知
        // Native.FreeLibraryAndExitThread(GlobalSettings.DllInstance, 0);
    }
}