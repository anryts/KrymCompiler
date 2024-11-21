using System.Text;
using Common;
using OurDartLangLexer.Lexer;
using YaltaLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    private int _currentTokenIndex = 0; //номер рядка, як і більшість, будемо вважати, що він починається з 0
    private readonly Lexer _lexer;
    private readonly int _lengthOfSymbolTable;
    private readonly List<Variable> _variableTable = new();
    private int _ident = -1; //для відступів

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
            Console.WriteLine(
                $"Parser: Синтаксичний аналіз завершено з помилкою на рядку {_lexer.TokenTable[_currentTokenIndex].NumLine}, {e.Message}");
        }
    }

    private void ParseStatementBlock()
    {
        ParseToken("{", "braces_op");
        ParseStatementList();
        ParseToken("}", "braces_op");
    }

    private void ParseStatementList()
    {
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: StatementList");
        while (_lexer.TokenTable[_currentTokenIndex].Lexeme != "}")
        {
            ParseStatement(_lexer.TokenTable[_currentTokenIndex]);
        }

        PrevIdent();
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
                NextIdent();
                Console.WriteLine(new string(' ', _ident) + "Parser: DeclarationStatement");
                ParseDeclarationStatement();
                PrevIdent();
                break;
            case "print":
                throw new NotImplementedException();
                break;
            case ";":
                ParseToken(";", "punct");
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
        var type = _lexer.TokenTable[_currentTokenIndex].Lexeme;
        NextIdent();
        ParseToken(type, "keyword");
        var variableName = _lexer.TokenTable[_currentTokenIndex].Lexeme;
        if (_variableTable.Any(v => v.Name == variableName))
        {
            throw new Exception($"Змінна {variableName} вже була оголошена");
        }

        //ну шо це таке, треба буде виправити
        ParseToken(variableName, "id");
        ParseToken("=", "assign_op");
        _ = ParseExpression();
        //Console.WriteLine($"Parser: DeclarationStatement: {type} {variableName} = {value.value}");

        _variableTable.Add(new Variable(variableName, type, "null"));
        PrevIdent();
    }

    /// <summary>
    /// Функція для розбору токенів
    /// Перевіряє чи вказана лексема lexeme зістрілася з токеном token
    /// </summary>
    /// <param name="lexeme"></param>
    /// <param name="token"></param>
    private void ParseToken(string lexeme, string token)
    {
        NextIdent();
        if (_currentTokenIndex >= _lengthOfSymbolTable)
        {
            throw new Exception("Неочікуваний кінець програми");
        }

        if (_lexer.TokenTable[_currentTokenIndex].Lexeme != lexeme ||
            _lexer.TokenTable[_currentTokenIndex].Type != token)
        {
            throw new Exception(
                $"Неочікуваний токен: {_lexer.TokenTable[_currentTokenIndex].Lexeme} на рядку {_currentTokenIndex} " +
                $"очікувалось {lexeme} типу {token}");
        }

        Console.WriteLine(new string(' ', _ident) +
                          $"Parser: ParseToken: В рядку {_lexer.TokenTable[_currentTokenIndex].NumLine} токен {_lexer.TokenTable[_currentTokenIndex].Lexeme} " +
                          $"типу {_lexer.TokenTable[_currentTokenIndex].Type} було розпізнано");
        _currentTokenIndex++;
        PrevIdent();
    }

    private void ParseAssignmentStatement()
    {
        //Assign = Id "=" Expression
        //check if the variable is in the table
        //wtf is going on here
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: AssignmentStatement");
        //TODO: look at this monster
        var variable = _variableTable.FirstOrDefault(v => v.Name == _lexer.TokenTable[_currentTokenIndex].Lexeme)
                       ?? throw new Exception(
                           $"Змінна {_lexer.TokenTable[_currentTokenIndex].Lexeme} не була оголошена");

        ParseToken(variable.Name, "id");
        ParseToken("=", "assign_op");
        //TODO: потрібно перевірити, щоб bool не присвоювався int або double
        var expression = ParseExpression();
        // if (expression.type != variable.Type)
        // {
        //     throw new Exception($"Cannot assign {expression.type} to {variable.Type}");
        // }

        //Console.WriteLine($"Parser: AssignmentStatement: {variable.Name} = {expression.value}");
        //rewrite the value of the variable

        variable.Value = expression.value;
        PrevIdent();
    }

    /// <summary>
    /// Yes, it's a string which represents a type, but it will be change in the future, i hope so :]]
    /// Також, вирази можуть бути арифметичними або булевими (але без поєднання між ними)
    /// </summary>
    /// <returns>a tuple, first - type, second - value</returns>
    private (string type, string value) ParseExpression()
        => IsBooleanExpression() ? ParseBoolExpr() : ParseArthmExpr();


    private (string type, string value) ParseBoolExpr()
    {
        //BoolExpr = RelationalExpr | BoolExpr "&&" RelationalExpr | BoolExpr "||" RelationalExpr
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: BoolExpr");
        var currentToken = _lexer.TokenTable[_currentTokenIndex];
        var left = ParseRelationalExpr();
        while (currentToken.Lexeme is "&&" or "||")
        {
            ParseToken(currentToken.Lexeme, currentToken.Type);
            var right = ParseRelationalExpr();

            //TODO: об'єднання булевих виразів і ще б різну кольорову гаму для консолі
        }

        PrevIdent();
        return ("", "");
    }

    private object ParseRelationalExpr()
    {
        //RelationalExpr = ArthmExpr RelationalOp ArthmExpr
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: RelationalExpr");
        var left = ParseArthmExpr();
        var currentToken = _lexer.TokenTable[_currentTokenIndex];
        if (currentToken.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            ParseToken(currentToken.Lexeme, currentToken.Type);
        }

        var right = ParseArthmExpr();
        PrevIdent();
        return null;
    }

    /// <summary>
    /// Глядімо в майбутнє, щоб взнати чи є вираз булевим
    /// </summary>
    /// <param name="currentTokenLexeme"></param>
    /// <returns></returns>
    private bool IsBooleanExpression()
    {
        var currentTokenLexeme = _lexer.TokenTable[_currentTokenIndex].Lexeme;

        if (currentTokenLexeme == "true" || currentTokenLexeme == "false")
        {
            return true;
        }

        if (currentTokenLexeme is "&&" or "||" or "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            return true; // Logical or relational operator
        }

        if (IsRelationalExpression())
        {
            return true;
        }

        return false;
    }

    private bool IsRelationalExpression()
    {
        var initialIndex = _currentTokenIndex;

        //парсимо можливий арифметичний вираз
        ParseArthmExpr();
        bool result = false;
        var token = _lexer.TokenTable[_currentTokenIndex];

        //якщо ми зустріли один з реляційних операторів, то це реляційний вираз
        if (token.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            result = true;
        }

        //повертаємо індекс назад
        _currentTokenIndex = initialIndex;
        return result;
    }


    private (string type, string value) ParseArthmExpr()
    {
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: ArthmExpr");
        ParseTerm();

        while (_lexer.TokenTable[_currentTokenIndex].Lexeme == "+" ||
               _lexer.TokenTable[_currentTokenIndex].Lexeme == "-")
        {
            ParseToken(_lexer.TokenTable[_currentTokenIndex].Lexeme, _lexer.TokenTable[_currentTokenIndex].Type);
            ParseTerm();
        }

        PrevIdent();
        return ("", ""); //TODO: змінити на щось нормальне
    }

    private void ParseTerm()
    {
        //Term = Factor | Term "*" Factor | Term "/" Factor
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: Term");
        var currentToken = _lexer.TokenTable[_currentTokenIndex];
        //якщо ми зустріли символ ;, то ми виходимо з цього методу
        ParseFactor();
        while (_lexer.TokenTable[_currentTokenIndex].Lexeme is "*" or "/")
        {
            ParseToken(_lexer.TokenTable[_currentTokenIndex].Lexeme, _lexer.TokenTable[_currentTokenIndex].Type);
            ParseFactor();
        }

        PrevIdent();
    }

    private void ParseFactor()
    {
        //Factor = Id | Number | "(" ArthmExpr ")"
        //маєш три опції, або змінна, або число
        var currentToken = _lexer.TokenTable[_currentTokenIndex];
        NextIdent();
        Console.WriteLine(new string(' ', _ident) + "Parser: Factor");
        if (currentToken.Type == "id")
        {
            if (_variableTable.All(v => v.Name != currentToken.Lexeme))
            {
                throw new Exception($"Змінна {currentToken.Lexeme} не була оголошена");
            }

            ParseToken(currentToken.Lexeme, "id");
            //_currentTokenIndex++;
        }

        if (currentToken.Type == "intnum" || currentToken.Type == "realnum")
        {
            ParseToken(currentToken.Lexeme, currentToken.Type);
            //_currentTokenIndex++;
        }

        if (currentToken.Lexeme is "true" or "false")
        {
            ParseToken(currentToken.Lexeme, "boolval");
            //_currentTokenIndex++;
        }

        if (_lexer.TokenTable[_currentTokenIndex].Lexeme == "(")
        {
            ParseToken("(", "brackets_op");
            ParseExpression();
            ParseToken(")", "brackets_op");
        }

        PrevIdent();
    }

    private void NextIdent()
    {
        _ident++;
    }

    private void PrevIdent()
    {
        _ident--;
    }
}