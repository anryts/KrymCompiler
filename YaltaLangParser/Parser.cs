using Common;
using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    private readonly Lexer _lexer;
    private readonly TokenParser _tokenParser;
    private readonly ExpressionParser expressionParser;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _tokenParser = new TokenParser(lexer);
        expressionParser = new ExpressionParser(lexer, _tokenParser);
    }

    /// <summary>
    /// Повертає результат у вигляді string, якщо виникла якась помилка
    /// Та додатково виводить до консолі результат
    /// </summary>
    /// <returns></returns>
    public string ParseProgram()
    {
        try
        {
            // Спершу ми читаємо правило main -> StatementBlock
            //ParseToken("main", "keyword");
            _tokenParser.ParseToken("main", "keyword");

            //Потім ми читаємо правило StatementBlock -> { StatementList }
            ParseStatementBlock();
            //Це перевірка, чи ми дочитали всі токени. Можлива така ситуація символи '}' дублються
            if (GlobalVars.CurrentTokenIndex != _lexer.TokenTable.Count)
            {
                throw new Exception("Символ '}' повторюється");
            }
            ParserOutput.WriteSuccess("Парсинг завершено успішно");
            return string.Empty;
        }
        catch (Exception e)
        {
            ParserOutput.WriteError($"Помилка парсингу: {e.Message}");
            return $"Помилка парсингу: {e.Message}";
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

        ParserOutput.WriteInfo("Parser: StatementList");
        ParserOutput.DecreaseIndent();
    }

    private void ParseStatement(Token token)
    {
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
            default:
                if (token.Type == "id")
                {
                    ParseAssignmentStatement();
                }
                else
                {
                    throw new Exception($"Неочікуваний токен: {token.Lexeme}");
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
        ParserOutput.WriteColoredLine("Parser: WhileStatement", ConsoleColor.Yellow);
        _tokenParser.ParseToken("while", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        expressionParser.ParseExpression("bool");
        _tokenParser.ParseToken(")", "brackets_op");
        ParseStatementBlock();
        ParserOutput.WriteColoredLine("Parser: WhileStatement", ConsoleColor.Yellow);
        ParserOutput.DecreaseIndent();
    }

    /// <summary>
    /// Перевіряємо when -> fallback конструкцію
    /// </summary>
    private void ParseIfStatement()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: IfStatement", ConsoleColor.Yellow);

        _tokenParser.ParseToken("when", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        _ = expressionParser.ParseExpression("bool");
        _tokenParser.ParseToken(")", "brackets_op");
        ParseStatementBlock();
        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "fallback")
        {
            ParseFallbackStatement();
        }

        ParserOutput.WriteColoredLine("Parser: IfStatement", ConsoleColor.Yellow);
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
        ParserOutput.WriteColoredLine("Parser: DeclarationStatement", ConsoleColor.Yellow);
        //Declaration = Type Id
        var currentType = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
        ParserOutput.IncreaseIndent();
        _tokenParser.ParseToken(currentType, "keyword");
        var variableName = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
        if (GlobalVars.VariableTable.Any(v => v.Name == variableName))
        {
            throw new Exception($"Змінна {variableName} вже була оголошена");
        }

        _tokenParser.ParseToken(variableName, "id");
        _tokenParser.ParseToken("=", "assign_op");
        var result = expressionParser.ParseExpression(currentType);
        GlobalVars.VariableTable.Add(new Variable(variableName, currentType, ""));
        _tokenParser.ParseToken(";", "punct");
        ParserOutput.DecreaseIndent();
        ParserOutput.WriteColoredLine("Parser: DeclarationStatement", ConsoleColor.Yellow);
        ParserOutput.DecreaseIndent();
    }

    private void ParseAssignmentStatement()
    {
        //Assign = Id "=" Expression
        //check if the variable is in the table
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: AssignmentStatement", ConsoleColor.Yellow);
        var variable = GlobalVars.VariableTable.FirstOrDefault(v => v.Name == _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme);
        if (variable == null)
        {
            throw new Exception($"Variable {_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme} is not declared");
        }
        var currentType = variable.Type;
        _tokenParser.ParseToken(variable.Name, "id");
        _tokenParser.ParseToken("=", "assign_op");

        var expression = expressionParser.ParseExpression(currentType);
        _tokenParser.ParseToken(";", "punct");
        ParserOutput.DecreaseIndent();
    }
}