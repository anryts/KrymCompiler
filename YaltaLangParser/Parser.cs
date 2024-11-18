using System.Text;
using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    private int _numLine = 1;
    private readonly Lexer _lexer;
    private readonly int _lengthOfSymbolTable;
    private

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _lengthOfSymbolTable = lexer.TokenTable.Count;
    }

    public void ParseProgram()
    {
        try
        {
            //TODO обробка таблиці від лексера
            //TODO Спершу ми читаємо правило main -> StatementBlock
            ParseToken("main", "keyword");

            //TODO Потім ми читаємо правило StatementBlock -> { StatementList }
            ParseStatementBlock();
            Console.WriteLine("Parser: Синтаксичний аналіз завершено успішно");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Parser: Синтаксичний аналіз завершено з помилкою на рядку {_numLine}, {e.Message}");
        }
    }

    private void ParseStatementBlock()
    {
        ParseToken("{", "symbol");
        ParseStatementList();
        ParseToken("}", "symbol");
    }

    private void ParseStatementList()
    {
        while (_lexer.TokenTable[_lengthOfSymbolTable].Lexeme != "}")
        {
            ParseStatement();
        }
    }

    private void ParseStatement()
    {
        //TODO Statement -> PrintStatement
        //                  | AssignmentStatement
        //                  | IfStatement
        //                  | WhileStatement
        //                  | PrintStatement

        switch (_lexer.TokenTable[].Lexeme)
        {
            case "print":
                ParsePrintStatement();
                break;
            case "if":
                ParseIfStatement();
                break;
            case "while":
                ParseWhileStatement();
                break;
            default:
                ParseAssignmentStatement();
                break;
        }
        {

        }

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