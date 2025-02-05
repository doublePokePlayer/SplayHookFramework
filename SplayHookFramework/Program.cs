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
            Native.AllocConsole();

            Task.Run(Start);
        }

        return true;
    }

    private static void Start()
    {
        var a = new ImGuiDx11Hook();
        a.Initial();
    }
}