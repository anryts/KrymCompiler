using Common;
using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    private readonly Lexer _lexer;
    private readonly int _lengthOfSymbolTable;
    private readonly List<Variable> _variableTable = new();
    private int _ident = -1; //для відступів
    private readonly TokenParser _tokenParser;
    private readonly ExpressionParser expressionParser;

    static string _currentType = "";

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _lengthOfSymbolTable = lexer.TokenTable.Count;

        _tokenParser = new TokenParser(lexer);
        expressionParser = new ExpressionParser(lexer, _tokenParser);
    }

    public void ParseProgram()
    {
        try
        {
            // Спершу ми читаємо правило main -> StatementBlock
            //ParseToken("main", "keyword");
            _tokenParser.ParseToken("main", "keyword");

            //Потім ми читаємо правило StatementBlock -> { StatementList }
            ParseStatementBlock();
            ParserOutput.WriteSuccess("Парсинг завершено успішно");
        }
        catch (Exception e)
        {
            ParserOutput.WriteError($"Помилка парсингу: {e.Message}");
        }
    }

    private void ParseStatementBlock()
    {
        _tokenParser.ParseToken("{", "braces_op");
        ParseStatementList();
        _tokenParser.ParseToken("}", "braces_op");
    }

    private void ParseStatementList()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteInfo("Parser: StatementList");
        while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme != "}")
        {
            ParseStatement(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
        }

        ParserOutput.DecreaseIndent();
    }

    private void ParseStatement(Token token)
    {
        //TODO: цей монстро-метод потрібно якось спростити, ну бо це жахіття якесь
        switch (token.Lexeme)
        {
            case "when":
                ParseIfStatement();
                break;
            case "while":
                ParseWhileStatement();
                break;
            case "int":
            case "double":
            case "bool":
                ParseDeclarationStatement();
                break;
            case "print":
                ParsePrintStatement();
                break;
            case "read":
                ParseReadStatement();
                break;
            case ";":
                _tokenParser.ParseToken(";", "punct");
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

    private void ParseReadStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: ReadStatement");
        _tokenParser.ParseToken("read", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        expressionParser.ParseExpression("void"); 

        //TODO: для читання не дуже підходить,
        //ми можемо щось типу такого написати read(123),
        //що буде неправильно, але це поки що не важливо

        _tokenParser.ParseToken(")", "brackets_op");
        _tokenParser.ParseToken(";", "punct");
        ParserOutput.DecreaseIndent();
    }


    /// <summary>
    /// Перевіряємо while -> { StatementBlock } конструкцію
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseWhileStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: WhileStatement");
        _tokenParser.ParseToken("while", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        expressionParser.ParseExpression("bool");
        _tokenParser.ParseToken(")", "brackets_op");
        ParseStatementBlock();
        ParserOutput.DecreaseIndent();
    }

    /// <summary>
    /// Перевіряємо when -> fallback конструкцію
    /// </summary>
    private void ParseIfStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteInfo("Parser: WhenStatement");
        Console.WriteLine(new string(' ', _ident) + "Parser: WhenStatement");

        _tokenParser.ParseToken("when", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        expressionParser.ParseExpression("bool");
        _tokenParser.ParseToken(")", "brackets_op");
        ParseStatementBlock();
        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "fallback")
        {
            ParseFallbackStatement();
        }

        ParserOutput.DecreaseIndent();
    }

    private void ParsePrintStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: PrintStatement");
        _tokenParser.ParseToken("print", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        expressionParser.ParseExpression("void");
        _tokenParser.ParseToken(")", "brackets_op");
        _tokenParser.ParseToken(";", "punct");
        ParserOutput.DecreaseIndent();
    }
    private void ParseFallbackStatement()
    {
        _tokenParser.ParseToken("fallback", "keyword");
        ParseStatementBlock();
    }

    /// <summary>
    /// Парсить деклараційні оператори
    /// int, double, bool
    /// і добавляє їх в таблицю змінних
    /// </summary>
    private void ParseDeclarationStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: DeclarationStatement");
        //Declaration = Type Id
        _currentType = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
        ParserOutput.IncreaseIndent();
        _tokenParser.ParseToken(_currentType, "keyword");
        var variableName = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
        if (_variableTable.Any(v => v.Name == variableName))
        {
            throw new Exception($"Змінна {variableName} вже була оголошена");
        }

        //ну шо це таке, треба буде виправити
        //_ParseToken(variableName, "id");
        _tokenParser.ParseToken(variableName, "id");
        _tokenParser.ParseToken("=", "assign_op");
        _tokenParser.ParseToken("=", "assign_op");
        var result = expressionParser.ParseExpression(_currentType);
        //Console.WriteLine($"Parser: DeclarationStatement: {type} {variableName} = {value.value}");

        //_variableTable.Add(new Variable(variableName, result.type, "null"));
        ParserOutput.DecreaseIndent();
        ParserOutput.DecreaseIndent();
    }

    ///// <summary>
    ///// Функція для розбору токенів
    ///// Перевіряє чи вказана лексема lexeme зістрілася з токеном token
    ///// </summary>
    ///// <param name="lexeme"></param>
    ///// <param name="token"></param>
    //private void ParseToken(string lexeme, string token)
    //{
    //    NextIdent();
    //    if (GlobalVars.CurrentTokenIndex >= _lengthOfSymbolTable)
    //    {
    //        throw new Exception("Неочікуваний кінець програми");
    //    }

    //    if (_lexer.TokenTable[_currentTokenIndex].Lexeme != lexeme ||
    //        _lexer.TokenTable[_currentTokenIndex].Type != token)
    //    {
    //        throw new Exception(
    //            $"Неочікуваний токен: {_lexer.TokenTable[_currentTokenIndex].Lexeme} на рядку {_currentTokenIndex} " +
    //            $"очікувалось {lexeme} типу {token}");
    //    }

    //    Console.WriteLine(new string(' ', _ident) +
    //                      $"Parser: ParseToken: В рядку {_lexer.TokenTable[_currentTokenIndex].NumLine} токен {_lexer.TokenTable[_currentTokenIndex].Lexeme} " +
    //                      $"типу {_lexer.TokenTable[_currentTokenIndex].Type} було розпізнано");
    //    _currentTokenIndex++;
    //    PrevIdent();
    //}

    private void ParseAssignmentStatement()
    {
        //Assign = Id "=" Expression
        //check if the variable is in the table
        //wtf is going on here
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteInfo("Parser: AssignmentStatement");
        //TODO: look at this monster
        var variable = expressionParser.IsVariableDeclared(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme);
        if (variable == null)
        {
            throw new Exception($"Variable {_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme} is not declared");
        }
        _currentType = variable.Type;
        _tokenParser.ParseToken(variable.Name, "id");
        _tokenParser.ParseToken("=", "assign_op");
        //TODO: потрібно перевірити, щоб bool не присвоювався int або double
        var expression = expressionParser.ParseExpression(_currentType);
        // if (expression.type != variable.Type)
        // {
        //     throw new Exception($"Cannot assign {expression.type} to {variable.Type}");
        // }

        //Console.WriteLine($"Parser: AssignmentStatement: {variable.Name} = {expression.value}");
        //rewrite the value of the variable

        expressionParser.UpdateVariable(variable.Name, expression.value);
        ParserOutput.DecreaseIndent();
    }

    ///// <summary>
    ///// Yes, it's a string which represents a type, but it will be change in the future, i hope so :]]
    ///// Також, вирази можуть бути арифметичними або булевими (але без поєднання між ними)
    ///// </summary>
    ///// <returns>a tuple, first - type, second - value</returns>
    //private (string type, string value) ParseExpression(string type)
    //    => IsBooleanExpression() ? ParseBoolExpr(type) : ParseArthmExpr(type);


    //private (string type, string value) ParseBoolExpr(string type)
    //{
    //    //BoolExpr = RelationalExpr | BoolExpr "&&" RelationalExpr | BoolExpr "||" RelationalExpr
    //    NextIdent();
    //    Console.WriteLine(new string(' ', _ident) + "Parser: BoolExpr");
    //    var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
    //    var left = ParseRelationalExpr();
    //    string right = "";
    //    while (currentToken.Lexeme is "&&" or "||")
    //    {
    //        ParseToken(currentToken.Lexeme, currentToken.Type);
    //        right = ParseRelationalExpr();

    //        //TODO: об'єднання булевих виразів і ще б різну кольорову гаму для консолі
    //    }

    //    PrevIdent();

    //    if (right == "")
    //    {
    //        if (!CheckCompatibility(left, _currentType))
    //        {
    //            throw new Exception($"Тип виразу {left} не сумісний");
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(right) && !CheckCompatibility(left, right))
    //    {
    //        if (right == "")
    //        {
    //            throw new Exception($"Тип виразу {left} не сумісний");
    //        }

    //        throw new Exception($"Типи виразів {left} та {right} не сумісні");
    //    }

    //    if (!string.IsNullOrWhiteSpace(right) && !CheckCompatibility(right, type))
    //    {
    //        throw new Exception($"Тип виразу {right} не сумісний");
    //    }

    //    return (_currentType, "");
    //}

    //private string ParseRelationalExpr()
    //{
    //    //RelationalExpr = ArthmExpr RelationalOp ArthmExpr
    //    NextIdent();
    //    Console.WriteLine(new string(' ', _ident) + "Parser: RelationalExpr");
    //    var left = ParseArthmExpr("void");
    //    var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
    //    string right = "";
    //    if (currentToken.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
    //    {
    //        ParseToken(currentToken.Lexeme, currentToken.Type);
    //        right = ParseArthmExpr("void").type;
    //    }

    //    PrevIdent();
    //    //ситуація, маєш 5 < 56 -> ну це ж буль, а між ними якийсь із операторів
    //    if (left.type.Equals(right))
    //    {
    //        return "bool"; //TODO: change this
    //    }
    //    return left.type;
    //}

    ///// <summary>
    ///// Глядімо в майбутнє, щоб взнати чи є вираз булевим
    ///// </summary>
    ///// <param name="currentTokenLexeme"></param>
    ///// <returns></returns>
    //private bool IsBooleanExpression()
    //{
    //    var currentTokenLexeme = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;

    //    if (currentTokenLexeme == "true" || currentTokenLexeme == "false")
    //    {
    //        return true;
    //    }

    //    if (currentTokenLexeme is "&&" or "||" or "==" or "!=" or "<" or "<=" or ">" or ">=")
    //    {
    //        return true; // Logical or relational operator
    //    }

    //    if (IsRelationalExpression())
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    //private bool IsRelationalExpression()
    //{
    //    var initialIndex = GlobalVars.CurrentTokenIndex;

    //    //парсимо можливий арифметичний вираз
    //    ParseArthmExpr("void");
    //    bool result = false;
    //    var token = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];

    //    //якщо ми зустріли один з реляційних операторів, то це реляційний вираз
    //    if (token.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
    //    {
    //        result = true;
    //    }

    //    //повертаємо індекс назад
    //    GlobalVars.CurrentTokenIndex = initialIndex;
    //    return result;
    //}


    //private (string type, string value) ParseArthmExpr(string type)
    //{
    //    NextIdent();
    //    Console.WriteLine(new string(' ', _ident) + "Parser: ArthmExpr");
    //    var leftType = ParseTerm();
    //    string rightType = "";
    //    while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "+" ||
    //           _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "-")
    //    {
    //        ParseToken(_lexer.TokenTable[_currentTokenIndex].Lexeme, _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type);
    //        rightType = ParseTerm();
    //    }


    //    //TODO: проблема перевірки при BooleanExpression
    //    PrevIdent();
    //    if (type == "void") //якщо тип не вказаний, то ми просто повертаємо тип виразу (для print, when, while)
    //    {
    //        return (leftType, "");
    //    }

    //    if (rightType == "")
    //    {
    //        if (!CheckCompatibility(leftType, _currentType))
    //        {
    //            throw new Exception($"Тип виразу {leftType} не сумісний");
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(leftType, rightType))
    //    {
    //        if (rightType == "")
    //        {
    //            throw new Exception($"Тип виразу {leftType} не сумісний");
    //        }

    //        throw new Exception($"Типи виразів {leftType} та {rightType} не сумісні");
    //    }

    //    if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(rightType, _currentType))
    //    {
    //        throw new Exception($"Тип виразу {rightType} не сумісний");
    //    }

    //    return (_currentType, "");
    //}

    ///// <summary>
    ///// Повертає тип виразу
    ///// </summary>
    ///// <returns></returns>
    //private string ParseTerm()
    //{
    //    //Term = Factor | Term "*" Factor | Term "/" Factor

    //    NextIdent();
    //    Console.WriteLine(new string(' ', _ident) + "Parser: Term");
    //    var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
    //    //якщо ми зустріли символ ;, то ми виходимо з цього методу
    //    string leftType = ParseFactor();
    //    while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme is "*" or "/")
    //    {
    //        var op = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
    //        ParseToken(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme, _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type);
    //        //Логіка перевірки на нуль, криво і косо, але воно працює
    //        if (op == "/" && (Convert.ToInt16(_lexer.TokenTable[_currentTokenIndex].Lexeme) == 0))
    //        {
    //            throw new Exception("Ділення на нуль");
    //        }
    //        ParseFactor();
    //    }

    //    PrevIdent();
    //    return leftType;
    //}

    ///// <summary>
    ///// TODO
    ///// </summary>
    ///// <returns>Тип виразу</returns>
    ///// <exception cref="Exception"></exception>
    //private string ParseFactor()
    //{
    //    //Factor = Id | Number | "(" ArthmExpr ")"
    //    //маєш три опції, або змінна, або число
    //    var currentToken = _lexer.TokenTable[_currentTokenIndex];
    //    string result = "";
    //    NextIdent();
    //    Console.WriteLine(new string(' ', _ident) + "Parser: Factor");
    //    if (currentToken.Type == "id")
    //    {
    //        if (_variableTable.All(v => v.Name != currentToken.Lexeme))
    //        {
    //            throw new Exception($"Змінна {currentToken.Lexeme} не була оголошена");
    //        }

    //        ParseToken(currentToken.Lexeme, "id");
    //        result = _variableTable.First(v => v.Name == currentToken.Lexeme).Type;
    //        //_currentTokenIndex++;
    //    }

    //    if (currentToken.Type == "intnum" || currentToken.Type == "realnum")
    //    {
    //        ParseToken(currentToken.Lexeme, currentToken.Type);
    //        result = currentToken.Type == "intnum" ? "int" : "double";
    //        //_currentTokenIndex++;
    //    }

    //    if (currentToken.Lexeme is "true" or "false")
    //    {
    //        ParseToken(currentToken.Lexeme, "boolval");
    //        result = "bool";
    //        //_currentTokenIndex++;
    //    }

    //    if (_lexer.TokenTable[_currentTokenIndex].Lexeme == "(")
    //    {
    //        ParseToken("(", "brackets_op");
    //        result = ParseExpression("").type;
    //        ParseToken(")", "brackets_op");
    //    }

    //    if (_lexer.TokenTable[_currentTokenIndex].Lexeme == "^")
    //    {
    //        ParseToken("^", "exp_op");
    //        //йдемо вправо рекурсивно
    //        var rightType = ParseFactor();

    //        if (result != rightType || result == "bool")
    //        {
    //            throw new Exception($"Parser: Невідповідність типам: {result} ^ {rightType}");
    //        }
    //    }


    //    PrevIdent();
    //    return result;
    //}

    //private void NextIdent()
    //{
    //    _ident++;
    //}

    //private void PrevIdent()
    //{
    //    _ident--;
    //}

    //private bool CheckCompatibility(string type1, string type2)
    //    => type1 == type2;
}