using OurDartLangLexer.Lexer;
using Xunit;

namespace LexerTests;


public class LexerTests
{
    [Theory]
    [InlineData("base.yt")]
    [InlineData("nested.yt")]
    [InlineData("misspeled.yt")]
    [InlineData("excess_syntax_elements.yt")]
    public void ProcessInputTest_True_IfNotException(string fileName)
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory,"TestData", fileName);

        if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

        var lexer = new Lexer();

        var sourceCodeArr = File.ReadAllText(filePath);

        lexer.ProcessInput(sourceCodeArr);

        Assert.Null(lexer.ErroMessage);
    }

    [Theory]
    [InlineData("error_lexeme.yt")]
    public void ProcessInputTest_False_IfException(string fileName)
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory,"TestData", fileName);

        if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

        var lexer = new Lexer();

        var sourceCodeArr = File.ReadAllText(filePath);

        lexer.ProcessInput(sourceCodeArr);

        //check if erorr_message is not empty
        //if not empty, then exception was thrown
        Assert.NotNull(lexer.ErroMessage);
    }

}