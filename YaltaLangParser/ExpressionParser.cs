using Common;
using OurDartLangLexer.Lexer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace YaltaLangParser;

public class ExpressionParser
{
    private readonly Lexer _lexer;
    private TokenParser _tokenParser;

    public ExpressionParser(Lexer lexer, TokenParser tokenParser)
    {
        _lexer = lexer;
        _tokenParser = tokenParser;
    }

    public (string type, string value) ParseExpression(string exptectedType)
    {

        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: Expression", ConsoleColor.DarkCyan);
        //(string type, string value) result = ("", "");
        var result = exptectedType switch
        {
            "bool" => ParseBooleanExpression(exptectedType),
            "int" or "double" => ParseArithmeticExpression(exptectedType),
            _ => IsBooleanExpression() ? ParseBooleanExpression(exptectedType) : ParseArithmeticExpression(exptectedType)
        };

        ParserOutput.WriteColoredLine("Parser: Expression", ConsoleColor.DarkCyan);
        ParserOutput.DecreaseIndent();
        return result;
    }

    private (string Type, string Value) ParseBooleanExpression(string expectedType)
    {
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: BooleanExpression", ConsoleColor.DarkYellow);
        // Parse boolean expression logic...
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        var leftType = ParseRelationalExpr();
        string rightType = "";
        while (currentToken.Lexeme is "&&" or "||")
        {
            GlobalVars.SetsOfOperations.Add(currentToken);
            _tokenParser.ParseToken(currentToken.Lexeme, currentToken.Type);
            rightType = ParseRelationalExpr();

            //TODO: об'єднання булевих виразів і ще б різну кольорову гаму для консолі
        }

        ParserOutput.WriteColoredLine("Parser: BooleanExpression", ConsoleColor.DarkYellow);
        ParserOutput.DecreaseIndent();

        if (expectedType == "void")
        {
            return (leftType, "");
        }

        if (rightType == "")
        {
            if (!CheckCompatibility(leftType, expectedType))
            {
                throw new Exception($"Тип виразу: {leftType} не сумісний, на рядку: {currentToken.NumLine}");
            }
        }

        if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(leftType, rightType))
        {
            if (rightType == "")
            {
                throw new Exception($"Тип виразу: {leftType} не сумісний, на рядку: {currentToken.NumLine}");
            }

            throw new Exception($"Типи виразів {leftType} та {rightType} не сумісні, на рядку: {currentToken.NumLine}");
        }

        if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(rightType, expectedType))
        {
            throw new Exception($"Тип виразу {rightType} не сумісний, на рядку: {currentToken.NumLine}");
        }

        return ("bool", ""); // Placeholder
    }

    private (string Type, string Value) ParseArithmeticExpression(string expectedType)
    {

        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: ArithmeticExpression", ConsoleColor.DarkYellow);
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        var leftType = ParseTerm();
        if (expectedType == "double" && leftType == "int")
        {
            leftType = "double";
        }
        string rightType = "";
        // Parse arithmetic expression logic...
        while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme is "+" or "-")
        {
            var op = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
            GlobalVars.SetsOfOperations.Add(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
            _tokenParser.ParseToken(op, _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Type);
            
            rightType = ParseTerm();

            if (expectedType == "double" && rightType == "int")
            {
                rightType= "double";
            }

            if (leftType is "bool"|| rightType is "bool")
            {
                throw new Exception($"Помилка: оператор '{op}' не може бути застосований до типу 'bool'.");
            }
        }


        ParserOutput.WriteColoredLine("Parser: ArithmeticExpression", ConsoleColor.DarkYellow);
        ParserOutput.DecreaseIndent();

        //TODO: проблема перевірки при BooleanExpression

        if (expectedType == "void") //якщо тип не вказаний, то ми просто повертаємо тип виразу (для print, when, while)
        {
            return (leftType, "");
        }

        if (rightType == "")
        {
            if (!CheckCompatibility(leftType, expectedType))
            {
                if (leftType == "int" && expectedType == "double")
                {
                    return (expectedType, "");
                }
                throw new Exception($"Тип виразу: {leftType} не сумісний, на рядку: {currentToken.NumLine}");
            }
        }

        if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(leftType, rightType))
        {
            if (rightType == "")
            {
                throw new Exception($"Тип виразу:  {leftType}  не сумісний, ня рядку: {currentToken.NumLine}");
            }

            throw new Exception($"Типи виразів {leftType} та {rightType} не сумісні, на рядку: {currentToken.NumLine}");
        }

        if (!string.IsNullOrWhiteSpace(rightType) && !CheckCompatibility(rightType, expectedType))
        {
            throw new Exception($"Тип виразу {rightType} не сумісний, на рядку: {currentToken.NumLine}");
        }

        return (expectedType, "");
    }


    private string ParseRelationalExpr()
    {
        // RelationalExpr = ArthmExpr RelationalOp ArthmExpr
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: RelationalExpr");
        var left = ParseArithmeticExpression("void");
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        (string type, string value) right = ("", "");
        if (currentToken.Lexeme is "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            GlobalVars.SetsOfOperations.Add(currentToken);
            _tokenParser.ParseToken(currentToken.Lexeme, currentToken.Type);
            right = ParseArithmeticExpression("void");
        }

        if (currentToken.Lexeme is "-" or "+" or "*" or "/")
        {
            throw new Exception("Очікувався реляційний оператор, але знайдено арифметичний");
        }
        //TODO: true + 2 -> повинно бути помилковим

        ParserOutput.WriteColoredLine("Parser: RelationalExpr", ConsoleColor.DarkYellow);
        ParserOutput.DecreaseIndent();
        //ситуація, маєш 5 < 56 -> ну це ж буль, а між ними якийсь із операторів
        if (left.Type.Equals(right.type))
        {
            return "bool"; //TODO: change this
        }
        return left.Type;
    }

    /// <summary>
    /// Глядімо в майбутнє, щоб взнати чи є вираз булевим
    /// </summary>
    /// <param name="currentTokenLexeme"></param>
    /// <returns></returns>
    private bool IsBooleanExpression()
    {
        var currentIndex = GlobalVars.SetsOfOperations.Count;
        ParserOutput.IncreaseIndent();
        ParserOutput.WriteColoredLine("Parser: IsBooleanExpression", ConsoleColor.Blue);
        var currentTokenLexeme = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
        bool result = false;
        if (currentTokenLexeme == "true" || currentTokenLexeme == "false")
        {
            result = true;
        }

        if (currentTokenLexeme is "&&" or "||" or "==" or "!=" or "<" or "<=" or ">" or ">=")
        {
            result = true; // Logical or relational operator
        }

        if (IsRelationalExpression())
        {
            result = true;
        }
        ParserOutput.WriteColoredLine($"Parser: IsBooleanExpression {result}", ConsoleColor.Blue);
        ParserOutput.DecreaseIndent();
        var currentIndex2 = GlobalVars.SetsOfOperations.Count;
        GlobalVars.SetsOfOperations.RemoveRange(currentIndex, GlobalVars.SetsOfOperations.Count - currentIndex);
        return result;
    }

    private bool IsRelationalExpression()
    {
        var initialIndex = GlobalVars.CurrentTokenIndex;

        //парсимо можливий арифметичний вираз
        //TODO: проблема, кожного разу перевіряємо, чи то буль і так далі, потрібно це сховати
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
        string leftType = ParseFactor();
        while (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme is "*" or "/")
        {
            var op = _lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme;
            GlobalVars.SetsOfOperations.Add(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
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
        //Factor = Id | Number | "(" ArthmExpr ")" | ("+" | "-") | Factor "^" Factor 
        //маєш три опції, або змінна, або число
        var currentToken = _lexer.TokenTable[GlobalVars.CurrentTokenIndex];
        string result = "";

        ParserOutput.IncreaseIndent();
        ParserOutput.WriteLine("Parser: Factor");


        // Якщо маємо унарний оператор
        if (currentToken.Lexeme == "-") //|| currentToken.Lexeme == "+")
        {
            GlobalVars.SetsOfOperations.Add(new Token(int.Parse(currentToken.NumLine), "NEG", "neg_op"));
            
            _tokenParser.ParseToken(currentToken.Lexeme, "add_op"); // Розпізнаємо унарний оператор
           
            var operandType = ParseFactor(); // Розбираємо операнд

            if (operandType == "bool")
            {
                throw new Exception($"Унарний оператор '{currentToken.Lexeme}' не може бути застосований до типу 'bool'.");
            }

            result = operandType; // Тип результату збігається з типом операнду
        }
        // Якщо токен - змінна
        else if (currentToken.Type == "id")
        {
            if (GlobalVars.VariableTable.All(v => v.Name != currentToken.Lexeme))
            {
                throw new Exception($"Змінна {currentToken.Lexeme} не була оголошена");
            }
            GlobalVars.SetsOfOperations.Add(new Token(int.Parse(currentToken.NumLine), currentToken.Lexeme, "r-val"));
            //GlobalVars.SetsOfOperations.Add(currentToken);
            _tokenParser.ParseToken(currentToken.Lexeme, "id");
           
            result = GlobalVars.VariableTable.First(v => v.Name == currentToken.Lexeme).Type;
        }

        if (currentToken.Type == "intnum" || currentToken.Type == "realnum")
        {
            GlobalVars.SetsOfOperations.Add(currentToken);
            _tokenParser.ParseToken(currentToken.Lexeme, currentToken.Type);
            
            result = currentToken.Type == "intnum" ? "int" : "double";
            //_currentTokenIndex++;
        }

        if (currentToken.Lexeme is "true" or "false")
        {
            GlobalVars.SetsOfOperations.Add(currentToken);
            _tokenParser.ParseToken(currentToken.Lexeme, "boolval");
            
            result = "bool";
            //_currentTokenIndex++;
        }

        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "(")
        {
            GlobalVars.SetsOfOperations.Add(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
            _tokenParser.ParseToken("(", "brackets_op");
            result = ParseExpression("void").type;
            GlobalVars.SetsOfOperations.Add(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
            _tokenParser.ParseToken(")", "brackets_op");
            
        }

        //Правоасоціативний оператор ^, для його обробки використовуємо рекурсію
        if (_lexer.TokenTable[GlobalVars.CurrentTokenIndex].Lexeme == "^")
        {
            GlobalVars.SetsOfOperations.Add(_lexer.TokenTable[GlobalVars.CurrentTokenIndex]);
            _tokenParser.ParseToken("^", "exp_op");
          
            //йдемо вправо рекурсивно
            var rightType = ParseFactor();

            //TODO: перевірка на типи, але у нас до степення піднесення тілки double
            if (/*result != rightType || */result == "bool")
            {
                throw new Exception($"Parser: Невідповідність типам: {result} ^ {rightType}");
            }
            result = rightType;
        }

        ParserOutput.DecreaseIndent();
        return result;
    }

    public Variable IsVariableDeclared(string name)
        => GlobalVars.VariableTable.FirstOrDefault(v => v.Name == name);

    public void AddVariable(Variable variable)
        => GlobalVars.VariableTable.Add(variable);

    public void UpdateVariable(string name, string value)
    {
        var variable = GlobalVars.VariableTable.FirstOrDefault(v => v.Name == name);
        if (variable == null) //TODO: investigate
        {
            throw new Exception($"Змінна {name} не була оголошена");
        }
        variable.Value = value;
    }
    
    private bool CheckCompatibility(string type1, string type2)
        => type1 == type2;
}


