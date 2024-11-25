using Common;
using OurDartLangLexer.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace YaltaLangParser;

public class ExpressionParser
{
    private readonly Lexer _lexer;
    private TokenParser _tokenParser;
    private List<Variable> variables = new List<Variable>();

    public ExpressionParser(Lexer lexer, TokenParser tokenParser)
    {
        _lexer = lexer;
        _tokenParser = tokenParser;
    }

    public (string type, string value) ParseExpression(string exptectedType)
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: Expression");
        var result = IsBooleanExpression() ? ParseBooleanExpression(exptectedType) : ParseArithmeticExpression(exptectedType);
        ParserOutput.DecreaseIndent();
        
        return result;
    }

    private (string Type, string Value) ParseBooleanExpression(string expectedType)
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: BooleanExpression");
        // Parse boolean expression logic...
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        var left = ParseRelationalExpr();
        string right = "";
        while (currentToken.Lexeme is "&&" or "||")
        {
            _tokenParser.ParseToken(currentToken.Lexeme, currentToken.Type);
            right = ParseRelationalExpr();

            //TODO: об'єднання булевих виразів і ще б різну кольорову гаму для консолі
        }
        ParserOutput.DecreaseIndent();
        return ("bool", ""); // Placeholder
    }

    private (string Type, string Value) ParseArithmeticExpression(string expectedType)
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: ArithmeticExpression");
        var leftType = ParseTerm();
        // Parse arithmetic expression logic...
        ParserOutput.DecreaseIndent();
        return (expectedType, ""); // Placeholder
    }

   
    private string ParseRelationalExpr()
    {
        // Parse relational expression logic...
        return ""; // Placeholder
    }

    /// <summary>
    /// Глядімо в майбутнє, щоб взнати чи є вираз булевим
    /// </summary>
    /// <param name="currentTokenLexeme"></param>
    /// <returns></returns>
    private bool IsBooleanExpression()
    {
        var currentTokenLexeme = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;

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
        var initialIndex = GlobalVars.CurrentTokenIndex;

        //парсимо можливий арифметичний вираз
        ParseArithmeticExpression("void");
        bool result = false;
        var token = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];

        //якщо ми зустріли один з реляційних операторів, то це реляційний вираз
        if (token.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            result = true;
        }

        //повертаємо індекс назад
        GlobalVars.CurrentTokenIndex = initialIndex;
        return result;
    }

    /// <summary>
    /// //Term = Factor | Term "*" Factor | Term "/" Factor
    /// </summary>
    /// <returns></returns>
    private string ParseTerm()
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: Term");
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        //якщо ми зустріли символ ;, то ми виходимо з цього методу
        string leftType = ParseFactor();
        while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme is "*" or "/")
        {
            var op = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
            _tokenParser.ParseToken(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme, _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type);
            //Логіка перевірки на нуль, криво і косо, але воно працює
            if (op == "/" && (Convert.ToInt16(_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme) == 0))
            {
                throw new Exception("Ділення на нуль");
            }
            ParseFactor();
        }

        ParserOutput.DecreaseIndent();
        return leftType;
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <returns>Тип виразу</returns>
    /// <exception cref="Exception"></exception>
    private string ParseFactor()
    {
        //Factor = Id | Number | "(" ArthmExpr ")"
        //маєш три опції, або змінна, або число
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        string result = "";
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: Factor");
        if (currentToken.Type == "id")
        {
            if (variables.All(v => v.Name != currentToken.Lexeme))
            {
                throw new Exception($"Змінна {currentToken.Lexeme} не була оголошена");
            }

            _tokenParser.ParseToken(currentToken.Lexeme, "id");
            result = variables.First(v => v.Name == currentToken.Lexeme).Type;
            //_currentTokenIndex++;
        }

        if (currentToken.Type == "intnum" || currentToken.Type == "realnum")
        {
            _tokenParser.ParseToken(currentToken.Lexeme, currentToken.Type);
            result = currentToken.Type == "intnum" ? "int" : "double";
            //_currentTokenIndex++;
        }

        if (currentToken.Lexeme is "true" or "false")
        {
            _tokenParser.ParseToken(currentToken.Lexeme, "boolval");
            result = "bool";
            //_currentTokenIndex++;
        }

        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "(")
        {
            _tokenParser.ParseToken("(", "brackets_op");
            result = ParseExpression("").type;
            _tokenParser.ParseToken(")", "brackets_op");
        }

        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "^")
        {
            _tokenParser.ParseToken("^", "exp_op");
            //йдемо вправо рекурсивно
            var rightType = ParseFactor();

            if (result != rightType || result == "bool")
            {
                throw new Exception($"Parser: Невідповідність типам: {result} ^ {rightType}");
            }
        }

        ParserOutput.DecreaseIndent();
        return result;
    }

    public Variable IsVariableDeclared(string name)
        => variables.FirstOrDefault(v => v.Name == name);

    public void AddVariable(Variable variable)
        => variables.Add(variable);

    public void UpdateVariable(string name, string value)
    {
        var variable = variables.FirstOrDefault(v => v.Name == name);
        if (variable == null)
        {
            throw new Exception($"Змінна {name} не була оголошена");
        }
        variable.Value = value;
    }
}

