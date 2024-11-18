using OurDartLangLexer.Lexer;
using Xunit;

namespace LexerTests;

public class ParserTests
{
    public class LexerTests
    {
        [Theory]
        [InlineData("base.yt")]
        [InlineData("nested.yt")]
        [InlineData("misspeled.yt")]
        [InlineData("excess_syntax_elements.yt")]
        public void ProcessInputTest_True_IfNotException(string fileName)
        {

        }

    }
}