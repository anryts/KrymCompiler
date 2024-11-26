using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaltaLangParser;

public static class ParserOutput
{
    private static int _indentLevel = 0;

    public static void IncreaseIndent() => _indentLevel++;

    public static void DecreaseIndent() => _indentLevel--;

    public static void WriteLine(string message)
    {
        Console.WriteLine($"{new string(' ', _indentLevel * 2)}{message}");
    }

    public static void WriteColoredLine(string message, ConsoleColor color)
    {
        //Якщо програма закриється, кольорова схема може залишитися у такому стані. тому скидуємо кожного разу
        Console.ResetColor();
        Console.ForegroundColor = color;
        WriteLine(message);
        Console.ResetColor();
    }
    public static void WriteInfo(string message) => WriteColoredLine(message, ConsoleColor.Cyan);
    public static void WriteSuccess(string message) => WriteColoredLine(message, ConsoleColor.Green);
    public static void WriteError(string message) => WriteColoredLine(message, ConsoleColor.Red);
}

