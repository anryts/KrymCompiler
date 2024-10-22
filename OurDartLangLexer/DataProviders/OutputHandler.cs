namespace OurDartLangLexer.DataProviders;

public class OutputHandler
{
    // Method to write the lexer output to the console
    public void WriteToConsole(List<(string lexeme, string tokenType)> tableOfSymbols)
    {
        Console.WriteLine("Line  Lexeme     Token Type");
        Console.WriteLine("-----------------------------------");
        foreach (var entry in tableOfSymbols)
        {
            Console.WriteLine(entry);
        }
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Output written to the console successfully.");
    }

    // Method to write the lexer output to a file
    public void WriteToFile(string filePath, List<(string lexeme, string tokenType)> tableOfSymbols)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Line  Lexeme     Token Type");
                writer.WriteLine("-----------------------------------");
                foreach (var entry in tableOfSymbols)
                {
                    writer.WriteLine(entry);
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