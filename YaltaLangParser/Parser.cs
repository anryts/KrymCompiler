using Common;
using Common.Extensions;
using OurDartLangLexer.Lexer;

namespace YaltaLangParser;

public class Parser
{
    public readonly Lexer _lexer;
    public readonly TokenParser _tokenParser;
    public readonly ExpressionParser expressionParser;

    private int LabelCount = 0; //костиль
    public List<Token> CodeTable { get; set; } = new List<Token>();
    public List<Label> LabelTable = new List<Label>();

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
    public int ParseProgram()
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
            return 0;
        }
        catch (Exception e)
        {
            ParserOutput.WriteError($"Помилка парсингу: {e.Message}");
            return 1;
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
        this.CodeTable.AddRange(GlobalVars.CompileToPostfix());
        this.CodeTable.Add(new Token(Convert.ToInt32(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].NumLine), "READ", "read"));
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
        GlobalVars.MSILOutput.AppendLine();
        var conditionLabelName = GenerateLabel();
        var statementLabeName = GenerateLabel();
        _tokenParser.ParseToken("while", "keyword");
        _tokenParser.ParseToken("(", "brackets_op");
        GlobalVars.MSILOutput.AppendLine("// Start the while loop");
        GlobalVars.MSILOutput.AppendLine($"{conditionLabelName}:");

        var currentTokenIndex = GlobalVars.CurrentTokenIndex;
        LabelTable.Add(new Label(conditionLabelName, this.CodeTable.Count)); // для переміщення на умову

        expressionParser.ParseExpression("bool");

        CodeTable.Add(new Token(0, conditionLabelName, "label"));
        var result = GlobalVars.CompileToPostfix();
        CodeTable.AddRange(result);
        GlobalVars.CompileToMSILFromPostfix(result);
        GlobalVars.MSILOutput.AppendLine($"brfalse {statementLabeName}");
        GlobalVars.MSILOutput.AppendLine();
        CodeTable.Add(new Token(0, statementLabeName, "label"));
        CodeTable.Add(new Token(0, "JF", "jf"));
        _tokenParser.ParseToken(")", "brackets_op");

        ParseStatementBlock();

        CodeTable.AddRange(GlobalVars.CompileToPostfix());
        CodeTable.Add(new Token(0, conditionLabelName, "label"));
        CodeTable.Add(new Token(0, "JMP", "jmp"));
        GlobalVars.MSILOutput.AppendLine();
        GlobalVars.MSILOutput.AppendLine("//Jump back to start of loop");
        GlobalVars.MSILOutput.AppendLine($"br {conditionLabelName}");
        CodeTable.Add(new Token(0, statementLabeName, "label"));
        LabelTable.Add(new Label(statementLabeName, this.CodeTable.Count)); // вихід із циклу
        GlobalVars.MSILOutput.AppendLine();
        GlobalVars.MSILOutput.AppendLine("//End of loop (exit point)");
        GlobalVars.MSILOutput.AppendLine($"{statementLabeName}:");
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
        var labelIf = GenerateLabel();
        _ = expressionParser.ParseExpression("bool");
        //string[] test = new string[GlobalVars.SetsOfOperations.Count];
        //GlobalVars.SetsOfOperations.CopyTo(test);
        var result = GlobalVars.CompileToPostfix();
        CodeTable.AddRange(result);
        GlobalVars.CompileToMSILFromPostfix(result);
        Console.WriteLine(GlobalVars.MSILOutput.ToString());
        CodeTable.Add(new Token(0, labelIf, "label"));
        GlobalVars.MSILOutput.AppendLine();
        GlobalVars.MSILOutput.AppendLine($"brfalse {labelIf}");
        CodeTable.Add(new Token(0, "JF", "jf"));
        GlobalVars.SetsOfOperations.Clear();
        //add jmp operation
        _tokenParser.ParseToken(")", "brackets_op");
        ParseStatementBlock();
        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "fallback")
        {
            LabelTable.Add(new Label(labelIf, this.CodeTable.Count + 2)); // це костиль, бо мені ліньки зараз його дороблювати
            var labelElse = GenerateLabel();
            GlobalVars.MSILOutput.AppendLine();
            GlobalVars.MSILOutput.AppendLine($"br {labelElse}");
            GlobalVars.MSILOutput.AppendLine();
            CodeTable.Add(new Token(0, labelElse, "label"));
            CodeTable.Add(new Token(0, "JMP", "jmp"));
            CodeTable.Add(new Token(0, labelIf, "label"));
            GlobalVars.MSILOutput.AppendLine($"{labelIf}:");
            ParseFallbackStatement();
            CodeTable.Add(new Token(0, labelElse, "label"));
            LabelTable.Add(new Label(labelElse, this.CodeTable.Count));
            GlobalVars.MSILOutput.AppendLine();
            GlobalVars.MSILOutput.AppendLine($"{labelElse}:");
        }
        else
        {
            LabelTable.Add(new Label(labelIf, this.CodeTable.Count)); // це костиль, бо мені ліньки зараз його дороблювати
            CodeTable.Add(new Token(0, labelIf, "label"));
            GlobalVars.MSILOutput.AppendLine($"{labelIf}:");
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
        var result = GlobalVars.CompileToPostfix();
        this.CodeTable.AddRange(result);
        GlobalVars.CompileToMSILFromPostfix(result);
        this.CodeTable.Add(new Token(Convert.ToInt32(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].NumLine), "PRINT", "print"));
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex-1];
        var typeOfVariable = GlobalVars.VariableTable.FirstOrDefault(v => v.Name == currentToken.Lexeme).Type;
        
        GlobalVars.MSILOutput.AppendLine($"call void [mscorlib]System.Console::WriteLine({typeOfVariable.ConvertToMSILType()})");
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
        var typeForMSIL = currentType switch
        {
            "int" => "i4",
            "double" => "r8",
            "bool" => "i4",
            _ => throw new Exception("Невідомий тип")
        };
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        _tokenParser.ParseToken(variableName, "id");
        CodeTable.Add(new Token(Convert.ToInt32(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].NumLine), variableName, "l-val"));
        _tokenParser.ParseToken("=", "assign_op");
        var result = expressionParser.ParseExpression(currentType);
        var postfixCode = GlobalVars.CompileToPostfix();
        CodeTable.AddRange(postfixCode);
        CodeTable.Add(new Token(Convert.ToInt32(currentToken.NumLine), "=", "assign_op"));
        GlobalVars.VariableTable.Add(new Variable(variableName, currentType, ""));

        GlobalVars.SetsOfOperations.Clear();
        GlobalVars.CompileToMSILFromPostfix(postfixCode);
        //MSIL example: stloc.0
        GlobalVars.MSILOutput.AppendLine($"stloc.{GlobalVars.VariableTable.Count - 1}");
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
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        var variable = GlobalVars.VariableTable.FirstOrDefault(v => v.Name == _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme);
        if (variable == null)
        {
            throw new Exception($"Variable {_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme} не оголошена");
        }
        var currentType = variable.Type;
        CodeTable.Add(new Token(Convert.ToInt32(currentToken.NumLine), variable.Name, "l-val"));
        //i think. here we do not need to put this var on stack
        //GlobalVars.MSILOutput.AppendLine($"ldloc.{GlobalVars.VariableTable.FindIndex(x => x.Name == variable.Name)}");
        Console.WriteLine(GlobalVars.MSILOutput.ToString());
        _tokenParser.ParseToken(variable.Name, "id");
        var currentToken_1 = _lexer.TokenTable[GlobalVars.CurrentTokenIndex]; // remember the token assign_op =
        _tokenParser.ParseToken("=", "assign_op");
        var expression = expressionParser.ParseExpression(currentType);
        //write into postfix
        var result = GlobalVars.CompileToPostfix();
        CodeTable.AddRange(result);
        CodeTable.Add(currentToken_1);
        GlobalVars.CompileToMSILFromPostfix(result);
        GlobalVars.MSILOutput.AppendLine($"stloc.{GlobalVars.VariableTable.FindIndex(x => x.Name == variable.Name)}");
        GlobalVars.SetsOfOperations.Clear();
        _tokenParser.ParseToken(";", "punct");
        ParserOutput.DecreaseIndent();
    }

    private string GenerateLabel()
    {
        return $"L{++LabelCount}";
    }
}