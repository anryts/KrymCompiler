using OurDartLangLexer.Lexer;

namespace OurDartLangLexer.DataProviders;

public static class OutputHandler
{
    /// <summary>
    /// Outputs the result of the lexical analyzer to the console
    /// </summary>
    /// <param name="tableOfSymbols">Result of the lexical analyzer</param>
    /// <param name="symbolTable">Symbol table</param>
    public static void WriteToConsole(List<Token> tableOfSymbols, SymbolTable symbolTable)
    {
        Console.WriteLine("Line  Lexeme     Token Type");
        Console.WriteLine("-----------------------------------");
        foreach (var entry in tableOfSymbols)
        {
            Console.WriteLine($"{entry.NumLine}\t{entry.Lexeme}\t{entry.Type}");
        }
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Symbol Table:");
        Console.WriteLine("ID  Lexeme");
        Console.WriteLine("-----------------------------------");
        foreach (var entry in symbolTable.Identifiers)
        {
            Console.WriteLine($"{entry.Value}\t{entry.Key}");
        }
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Constant Table:");
        Console.WriteLine("ID  Lexeme  Type");
        Console.WriteLine("-----------------------------------");
        foreach (var entry in symbolTable.Constants)
        {
            Console.WriteLine($"{entry.Value.Item2}\t{entry.Key}\t{entry.Value.Item1}");
        }
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Output written to the console successfully.");
    }

    /// <summary>
    /// Outputs the result of the lexical analyzer to a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="tableOfSymbols">Result of the lexical analyzer</param>
    /// <param name="symbolTable">Symbol table</param>
    public static void WriteToFile(string filePath, List<Token> tableOfSymbols, SymbolTable symbolTable)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Line  Lexeme     Token Type");
                writer.WriteLine("-----------------------------------");
                foreach (var entry in tableOfSymbols)
                {
                    writer.WriteLine($"{entry.NumLine}\t{entry.Lexeme}\t{entry.Type}");
                }
                writer.WriteLine("-----------------------------------");
                writer.WriteLine("Symbol Table:");
                writer.WriteLine("ID  Lexeme");
                writer.WriteLine("-----------------------------------");
                foreach (var entry in symbolTable.Identifiers)
                {
                    writer.WriteLine($"{entry.Value}\t{entry.Key}");
                }
                writer.WriteLine("-----------------------------------");
                writer.WriteLine("Constant Table:");
                writer.WriteLine("ID  Lexeme  Type");
                writer.WriteLine("-----------------------------------");
                foreach (var entry in symbolTable.Constants)
                {
                    writer.WriteLine($"{entry.Value.Item2}\t{entry.Key}\t{entry.Value.Item1}");
                }
                writer.WriteLine("-----------------------------------");
                writer.WriteLine("Output written to the file successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }
}