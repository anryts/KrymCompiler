using System.Text;
using Common;
using OurDartLangLexer.Lexer;
using YaltaLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    private int _numLine = 1;
    private readonly Lexer _lexer;
    private readonly int _lengthOfSymbolTable;
    private readonly List<Variable> _variableTable = new();

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _lengthOfSymbolTable = lexer.TokenTable.Count;
    }

    public void ParseProgram()
    {
        try
        {
            //TODO: обробка таблиці від лексера
            //TODO: Спершу ми читаємо правило main -> StatementBlock
            ParseToken("main", "keyword");

            //TODO: Потім ми читаємо правило StatementBlock -> { StatementList }
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
        while (_lexer.TokenTable[_numLine].Lexeme != "}")
        {
            ParseStatement(_lexer.TokenTable[_numLine]);
            _numLine++;
        }
    }

    private void ParseStatement(Token token)
    {
        switch (token.Lexeme)
        {
            case "if":
                throw new NotImplementedException();
                break;
            case "while":
                throw new NotImplementedException();
                break;
            case "int":
            case "double":
            case "bool":
                ParseDeclarationStatement();
                break;
            case "print":
                throw new NotImplementedException();
                break;
            default:
                if (token.Type == "id")
                {
                    ParseAssignmentStatement();
                }
                else
                {
                    throw new Exception($"Unexpected token: {token.Lexeme}");
                }

                break;
        }
    }

    /// <summary>
    /// Парсить деклараційні оператори
    /// int, double, bool
    /// і добавляє їх в таблицю змінних
    /// </summary>
    private void ParseDeclarationStatement()
    {
        //Declaration = Type Id
        var type = _lexer.TokenTable[_numLine].Lexeme;
        ParseToken(type, "keyword");
        var variableName = _lexer.TokenTable[_numLine].Lexeme;
        ParseToken(variableName, "id");
        ParseToken("=", "assign_op");
        var value = ParseExpression();
        Console.WriteLine($"Parser: DeclarationStatement: {type} {variableName} = {value.value}");
        _variableTable.Add(new Variable(variableName, type, value.value));
    }

    /// <summary>
    /// Функція для розбору токенів
    /// Перевіряє чи вказана лексема lexeme зістрілася з токеном token
    /// </summary>
    /// <param name="lexeme"></param>
    /// <param name="token"></param>
    private void ParseToken(string lexeme, string token)
    {
        _numLine++;
        if (_numLine >= _lengthOfSymbolTable)
        {
            throw new Exception("Unexpected end of file");
        }

        if (_lexer.TokenTable[_numLine].Lexeme != lexeme || _lexer.TokenTable[_numLine].Type != token)
        {
            throw new Exception($"Unexpected token: {_lexer.TokenTable[_numLine].Lexeme}");
        }
    }

    private void ParseAssignmentStatement()
    {
        //Assign = Id "=" Expression
        //check if the variable is in the table
        //wtf is going on here
        //TODO: look at this monster
        var variable = _variableTable.FirstOrDefault(v => v.Name == _lexer.TokenTable[_numLine].Lexeme)
                       ?? throw new Exception($"Variable {_lexer.TokenTable[_numLine].Lexeme} is not declared");

        ParseToken("id", "id");
        ParseToken("=", "assign_op");
        //TODO: потрібно перевірити, щоб bool не присвоювався int або double
        var expression = ParseExpression();
        if (expression.type != variable.Type)
        {
            throw new Exception($"Cannot assign {expression.type} to {variable.Type}");
        }

        Console.WriteLine($"Parser: AssignmentStatement: {variable.Name} = {expression.value}");
        //rewrite the value of the variable

        variable.Value = expression.value;
    }

    /// <summary>
    /// Yes, it's a string which represents a type, but it will be change in the future, i hope so :]]
    /// </summary>
    /// <returns>a tuple, first - type, second - value</returns>
    private (string type, string value) ParseExpression()
    {
        //Expression = ArthnExpr | BoolExpr
        var currentToken = _lexer.TokenTable[_numLine];
        if (IsBooleanOperator(currentToken.Lexeme))
        {
            return ParseBoolExpr();
        }

        return ParseArthmExpr();
    }

    private (string type, string value) ParseBoolExpr()
    {
        throw new NotImplementedException();
    }

    private bool IsBooleanOperator(string currentTokenLexeme)
        => currentTokenLexeme == "==" || currentTokenLexeme == "!=" || currentTokenLexeme == ">" ||
           currentTokenLexeme == "<" || currentTokenLexeme == ">=" || currentTokenLexeme == "<=" ||
           currentTokenLexeme == "&&" || currentTokenLexeme == "||" || currentTokenLexeme == "!" ||
           currentTokenLexeme == "true" || currentTokenLexeme == "false";


    private (string type, string value) ParseArthmExpr()
    {
        //ArthmExpr = Term | ArthmExpr "+" Term | ArthmExpr "-" Term
        var currentToken = _lexer.TokenTable[_numLine];
        //йдемо далі, до того часу поки не зустрінем символ ;
        ParseTerm();
        while (currentToken.Lexeme != ";")
        {
            if (currentToken.Lexeme == "+" || currentToken.Lexeme == "-")
            {
                ParseToken(currentToken.Lexeme, "add_op");
            }

            //Повідомляємо про помилку із типами у самому методі ParseTerm
            var term = ParseTerm();
            Console.WriteLine($"Parser: ArthmExpr: {term}");
            currentToken = _lexer.TokenTable[_numLine];
        }
        _numLine++;

        //TODO: потрібно визначити тип виразу і його значення
        return ("int", "0");
    }

    private object ParseTerm()
    {
        //Term = Factor | Term "*" Factor | Term "/" Factor
        var currentToken = _lexer.TokenTable[_numLine];
        ParseFactor();
        while (currentToken.Lexeme != "+" && currentToken.Lexeme != "-" && currentToken.Lexeme != ";")
        {
            if (currentToken.Lexeme == "*" || currentToken.Lexeme == "/")
            {
                ParseToken(currentToken.Lexeme, "arithm_op");
            }

            var factor = ParseFactor();
            Console.WriteLine($"Parser: Term: {factor}");
            currentToken = _lexer.TokenTable[_numLine];
        }

        return 0;
    }

    private object ParseFactor()
    {
        //Factor = Id | Number | "(" ArthmExpr ")"
        //маєш три опції, або змінна, або число
        var currentToken = _lexer.TokenTable[_numLine];
        if (currentToken.Type == "id" )
        {
            ParseToken(currentToken.Lexeme, "id");
            return currentToken.Lexeme;
        }

        if (currentToken.Lexeme == "int" || currentToken.Lexeme == "double")
        {
            ParseToken(currentToken.Lexeme, "keyword");
            return currentToken.Lexeme;
        }

        if (currentToken.Lexeme == "(")
        {
            ParseToken("(", "brackets_op");
            var arthmExpr = ParseArthmExpr();
            ParseToken(")", "brackets_op");
            return arthmExpr;
        }

        throw new Exception($"Unexpected token: {currentToken.Lexeme}");
    }
}