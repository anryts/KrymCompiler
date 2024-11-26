using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class TokenParser(Lexer lexer)
{
    private readonly int _lengthOfSymbolTable = lexer.TokenTable.Count;

    public int ParseToken(string lexeme, string token)
    {
        if (GlobalVars.CurrentTokenIndex > _lengthOfSymbolTable)
        {
            throw new Exception("Вийшов за індекс");
        }

        ParserOutput.IncreaseIndent();

        if (lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme != lexeme ||
            lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type != token)
        {
            throw new Exception(
                $"Неочікуваний токен: {lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme} на рядку {GlobalVars.CurrentTokenIndex} " +
                $"очікувалось {lexeme} типу {token}");
        }

        ParserOutput.WriteLine($"Parser: ParseToken: В рядку {lexer.TokenTable[GlobalVars.CurrentTokenIndex].NumLine} токен {lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme} " +
                      $"типу {lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type} було розпізнано");
        ParserOutput.DecreaseIndent();

        GlobalVars.CurrentTokenIndex++;

        return GlobalVars.CurrentTokenIndex;
    }
}
