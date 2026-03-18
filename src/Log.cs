using System;

namespace UserConfigSaver;

public static class Log
{
    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[UserConfigSaver] {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[UserConfigSaver] {message}");
        Console.ResetColor();
    }
}