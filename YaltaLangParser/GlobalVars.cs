using Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
    { "||", 1 }, // Логічне OR
    { "&&", 2 }, // Логічне AND
    { "==", 3 }, { "!=", 3 }, // Рівність і нерівність
    { "<", 4 }, { ">", 4 }, { "<=", 4 }, { ">=", 4 }, // Порівняння
    { "+", 5 }, { "-", 5 }, // Додавання і віднімання
    { "*", 6 }, { "/", 6 }, { "%", 6 }, // Множення, ділення, модуль
    { "^", 7 }, // Піднесення до степеня
    { "NEG", 8 }, { "!", 8 } // Унарні оператори з найвищим пріоритетом
};

    public static List<Variable> VariableTable { get; set; } = new List<Variable>();
    public static List<Token> SetsOfOperations { get; set; } = new List<Token>();

    /// <summary>
    /// Приводить програму до постфіксної форми
    /// і вертає стек цього дійства
    /// і очищує стек викликів
    /// </summary>
    public static List<Token> CompileToPostrifx()
    {
        var output = new List<Token>();
        var operatorStack = new Stack<Token>();

        //ІПН, дякую Грузу, що вчив мене цьому
        foreach (var operation in GlobalVars.SetsOfOperations)
        {
            if (OperatorPrecedence.ContainsKey(operation.Lexeme))
            {
                while (operatorStack.Count > 0 && OperatorPrecedence[operatorStack.Peek().Lexeme] >= OperatorPrecedence[operation.Lexeme])
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
}
