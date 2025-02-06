using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SplayHookFramework.Misc;

public class Debug
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("DEBUG")]
    public static void DebugWriteLine(string text)
    {
        Console.WriteLine($"[{DateTime.Now:O}] {text}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLine(string text)
    {
        Console.WriteLine($"[{DateTime.Now:O}] {text}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:O}] {text}");
        Console.ResetColor();
    }
}