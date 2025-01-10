using OurDartLangLexer.Lexer;
using Xunit;
using YaltaLangLexer.DataProviders;
using YaltaLangParser;

namespace LexerTests;

public class ParserTests
{
    public class LexerTests
    {
        [Theory]
        [InlineData("test.yt")]
        [InlineData("nested.yt")]
        //[InlineData("misspelled.yt")] // Corrected spelling
        //[InlineData("excess_syntax_elements.yt")]
        public void ProcessInputTest_True_IfNotException(string fileName)
        {
            // Get the project directory dynamically
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            // Use the fileName passed from InlineData
            string filePath = Path.Combine(projectDirectory, "ExamplesOfCode", "ForParserTests", fileName);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new Exception("File not found: " + filePath);
            }

            // Initialize the lexer
            var lexer = new Lexer();

            // Read the file content
            var sourceCodeArr = File.ReadAllText(filePath);

            // Process the input with the lexer
            lexer.ProcessInput(sourceCodeArr);

            // Write the lexer tables to the console
            OutputHandler.WriteToConsole(lexer.TokenTable, lexer.SymbolTable, lexer.ErrorTable);

            // Initialize and execute the parser
            var parser = new Parser(lexer);
            var result = parser.ParseProgram();

            Assert.Equal(0, result);
        }
    }
}