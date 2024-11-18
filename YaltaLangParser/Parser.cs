using System.Text;
using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    public int NumLine = 1;
    private readonly Lexer _lexer;
    private readonly int _lengthOfSymbolTable;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _lengthOfSymbolTable = lexer.TokenTable.Count;
    }

    public void ParseProgram()
    {
        //TODO обробка таблиці від лексера
        //TODO Спершу ми читаємо правило main -> {StatementList}
        ParseToken("main", "keyword");
        throw new NotImplementedException();

        //TODO імплементація таблиці розбору
    }

    /// <summary>
    /// Функція для розбору токенів
    /// Перевіряє чи вказана лексема lexeme зістрілася з токеном token
    /// </summary>
    /// <param name="lexeme"></param>
    /// <param name="token"></param>
    private void ParseToken(string lexeme, string token)
    {
        
    }
}