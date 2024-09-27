using CliFx.Infrastructure;

namespace SingleExe.Tool.Extensions;

internal static class ConsoleExtension
{
    public static void WriteInformation(this IConsole console, string text) => console.Write(ConsoleColor.White, text);

    public static void WriteSuccess(this IConsole console, string text) => console.Write(ConsoleColor.Green, text);

    public static void WriteWarning(this IConsole console, string text) => console.Write(ConsoleColor.Yellow, text);

    public static void WriteError(this IConsole console, string text) => console.Write(ConsoleColor.Red, text);

    public static void Write(this IConsole console, ConsoleColor color, string text)
    {
        var old = console.ForegroundColor;
        console.ForegroundColor = color;
        console.Output.WriteLine(text);
        console.ForegroundColor = old;
    }
}
