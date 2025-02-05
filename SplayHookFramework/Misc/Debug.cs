using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SplayHookFramework.Misc;

public class Debug
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("DEBUG")]
    public static void DebugWriteLine(string text)
    {
        Console.WriteLine(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLine(string text)
    {
        Console.WriteLine(text);
    }
}