using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YaltaLangParser;

public static class GlobalVars
{
    public static int CurrentTokenIndex { get; set; }

    private static readonly Dictionary<string, int> OperatorPrecedence = new Dictionary<string, int>
{
    { "(", 0 },  // Дужки мають найнижчий пріоритет
    { ")", 0 },
    { "==", 3 }, { "!=", 3 }, // Рівність і нерівність
    { "<", 4 }, { ">", 4 }, { "<=", 4 }, { ">=", 4 }, // Порівняння
    { "+", 5 }, { "-", 5 }, // Додавання і віднімання
    { "*", 6 }, { "/", 6 }, // Множення, ділення, модуль
    { "^", 7 }, // Піднесення до степеня
    { "NEG", 8 }, { "!", 8 } // Унарні оператори з найвищим пріоритетом
};

    public static List<Variable> VariableTable { get; set; } = new List<Variable>();
    public static List<Token> SetsOfOperations { get; set; } = new List<Token>();
    public static StringBuilder MSILOutput { get; set; } = new StringBuilder();

    /// <summary>
    /// Приводить програму до постфіксної форми
    /// і вертає стек цього дійства
    /// і очищує стек викликів
    /// </summary>
    public static List<Token> CompileToPostfix()
    {
        var output = new List<Token>();
        var operatorStack = new Stack<Token>();

        //ІПН, дякую Грузу, що вчив мене цьому
        foreach (var operation in GlobalVars.SetsOfOperations)
        {
            if (OperatorPrecedence.ContainsKey(operation.Lexeme))
            {
                //Обробка лівосторонніх операторів
                while (operatorStack.Count > 0 &&
                      (OperatorPrecedence[operatorStack.Peek().Lexeme] > OperatorPrecedence[operation.Lexeme] ||
                       (OperatorPrecedence[operatorStack.Peek().Lexeme] == OperatorPrecedence[operation.Lexeme] &&
                        operation.Lexeme != "^")))
                {
                    output.Add(operatorStack.Pop());
                }
                operatorStack.Push(operation);
            }
            else if (operation.Lexeme == "(")
            {
                operatorStack.Push(operation);
            }
            else if (operation.Lexeme == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek().Lexeme != "(")
                {
                    output.Add(operatorStack.Pop());
                }
                operatorStack.Pop();
            }
            else
            {
                output.Add(operation);
            }
        }

        foreach (var operation in operatorStack)
        {
            output.Add(operation);
        }
        SetsOfOperations.Clear();
        return [.. output];
    }

    /// <summary>
    /// Робить з постфіксного коду MSIL код
    /// </summary>
    /// <param name="postfixCode"></param>
    public static void CompileToMSILFromPostfix(List<Token> postfixCode)
    {
        foreach (var token in postfixCode)
        {
            switch (token.Type)
            {
                case "l-val":
                    {
                        //msilCode.AppendLine($"ldloc.{index}");
                        break;
                    }
                case "r-val":
                    {
                        int variableIndex = VariableTable.FindIndex(x => x.Name == token.Lexeme);
                        GlobalVars.MSILOutput.AppendLine($"ldloc.{variableIndex}");
                        break;
                    }
                case "neg_op":
                    {
                        GlobalVars.MSILOutput.AppendLine("neg");
                        break;
                    }
                //TODO: Потрібно правильно підтягувати типи
                case "intnum" or "realnum" or "boolval":
                    {
                        var type = token.Type switch
                        {
                            "intnum" => "i4",
                            "realnum" => "r8",
                            "boolval" => "i4",
                            _ => throw new Exception("Invalid type")
                        };
                        //TODO: глянь на це
                        if (type == "i4" && token.Lexeme == "true")
                        {
                            GlobalVars.MSILOutput.AppendLine("ldc.i4.1");
                            break;
                        }
                        if (type == "i4" && token.Lexeme == "false")
                        {
                            GlobalVars.MSILOutput.AppendLine("ldc.i4.0");
                            break;
                        }
                        GlobalVars.MSILOutput.AppendLine($"ldc.{type} {token.Lexeme}");
                        break;
                    }
                case "rel_op":
                    {
                        //TODO: Потрібно правильно підтягувати типи
                        GlobalVars.MSILOutput.AppendLine(token.Lexeme switch
                        {
                            "==" => "ceq",
                            "!=" => "ceq\nldc.i4.0\ncgt",
                            ">" => "cgt",
                            "<" => "clt",
                            ">=" => "clt\nldc.i4.0\ncgt",
                            "<=" => "cgt\nldc.i4.0\ncgt",
                            _ => throw new Exception("Invalid operation")
                        });
                        break;
                    }
                case "add_op":
                    {
                        GlobalVars.MSILOutput.AppendLine(token.Lexeme switch
                        {
                            "+" => "add",
                            "-" => "sub",
                            _ => throw new Exception("Invalid operation")
                        });
                        break;
                    }
                case "mult_op":
                    {
                        GlobalVars.MSILOutput.AppendLine(token.Lexeme switch
                        {
                            "*" => "mul",
                            "/" => "div",
                            _ => throw new Exception("Invalid operation")
                        });
                        break;
                    }
                case "exp_op":
                    {
                        GlobalVars.MSILOutput.AppendLine("call float64 [mscorlib]System.Math::Pow(float64, float64)");
                        break;
                    }
                //case "jf":
                //    {
                //        var labelName = CodeTable[tokenIndex - 1].Lexeme;
                //        GlobalVars.MSILOutput.AppendLine($"brfalse {labelName}");
                //        break;
                //    }
                //case "jmp":
                //    {
                //        msilCode.AppendLine($"br {token.Lexeme}");
                //        break;
                //    }
                case "assign_op":
                    {
                        var variableIndex = VariableTable.FindIndex(x => x.Name == token.Lexeme);
                        GlobalVars.MSILOutput.AppendLine($"stloc.{variableIndex}");
                        break;
                    }
                case "print":
                    {
                        var index = VariableTable.FindIndex(x => x.Name == token.Lexeme);
                        var variableType = VariableTable[index].Type;
                        GlobalVars.MSILOutput.AppendLine(variableType switch
                        {
                            "int" => "call void [mscorlib]System.Console::WriteLine(int32)",
                            "double" => "call void [mscorlib]System.Console::WriteLine(float64)",
                            "bool" => "call void [mscorlib]System.Console::WriteLine(bool)",
                            _ => throw new Exception("Invalid type")
                        });
                        break;
                    }
                case "read":
                    {
                        GlobalVars.MSILOutput.AppendLine(" call string [mscorlib]System.Console::ReadLine()");
                        GlobalVars.MSILOutput.AppendLine("stloc");
                        break;
                    }
            }
        }
    }
}
