namespace SplayHookFramework;

public static class GlobalSettings
{
    public static bool UseConsole { get; set; }

    public static IntPtr DllInstance { get; set; }

    public static bool Running { get; set; } = true;
}