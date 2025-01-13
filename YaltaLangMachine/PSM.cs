using Common;
using Common.Enums;
using Common.Extensions;
using OurDartLangLexer.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaltaLangParser;

namespace YaltaLangMachine;

public class PSM(List<Label> labels, List<Variable> variables, List<Token> tokens)
{
    public List<Label> LabelTable { get; set; } = labels;
    public List<Variable> VariableTable { get; set; } = variables;
    public List<Token> CodeTable { get; set; } = tokens;

    public void ParsePostfixProgram()
    {
        //GOD method in action
        var operationStack = new Stack<Token>();
        int currentInstructionIndex = 0;
        while (currentInstructionIndex < CodeTable.Count)
        {
            var item = CodeTable[currentInstructionIndex];
            switch (item.Type)
            {
                case "l-val":
                    {
                        operationStack.Push(item);
                        break;
                    }
                case "r-val":
                    {
                        operationStack.Push(item);
                        break;
                    }
                case "neg_op":
                    {
                        var right = operationStack.Pop();
                        //TODO: bool value
                        var result = right.Type == "intnum" ? -int.Parse(right.Lexeme) : -double.Parse(right.Lexeme);
                        operationStack.Push(new Token(0, result.ToString(), right.Type));
                        break;
                    }
                case "intnum" or "realnum" or "boolval":
                    {
                        operationStack.Push(item);
                        break;
                    }
                case "rel_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();

                        right = GetTokenValue(right);
                        left = GetTokenValue(left);

                        var result = PerformRelOp(left, right, item.Lexeme.GetTypeOfOperation());
                        operationStack.Push(result);
                        break;
                    }
                case "add_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();

                        right = GetTokenValue(right);
                        left = GetTokenValue(left);

                        var higherType =
                            right.Type == "realnum" || right.Type == "double" || left.Type == "double" ||
                            left.Type == "realnum"
                                ? "realnum"
                                : "intnum";

                        var rightValue = GetValueFromToken(right);
                        var leftValue = GetValueFromToken(left);
                        //TODO: спрости це все
                        switch (item.Lexeme)
                        {
                            case "+":
                                {
                                    var result = rightValue + leftValue;
                                    operationStack.Push(new Token(0, result.ToString(), higherType));
                                    break;
                                }
                            case "-":
                                {
                                    var result = rightValue - leftValue;
                                    operationStack.Push(new Token(0, result.ToString(), higherType));
                                    break;
                                }
                        }

                        break;
                    }
                case "mult_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();
                        switch (item.Lexeme)
                        {
                            case "*":
                                {
                                    var result = right.Type == "intnum"
                                        ? int.Parse(right.Lexeme) * int.Parse(left.Lexeme)
                                        : double.Parse(right.Lexeme) * double.Parse(left.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), right.Type));
                                    break;
                                }
                            case "/":
                                {
                                    var result = right.Type == "intnum"
                                        ? int.Parse(left.Lexeme) / int.Parse(right.Lexeme)
                                        : double.Parse(left.Lexeme) / double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), right.Type));
                                    break;
                                }
                        }

                        break;
                    }
                case "exp_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();

                        var result = Math.Pow(double.Parse(left.Lexeme), double.Parse(right.Lexeme));
                        operationStack.Push(new Token(0, result.ToString(), "realnum"));
                        break;
                    }
                case "jf":
                    {
                        var condition = operationStack.Pop();
                        //достань тип змінної у випадку, якщо це boolval
                        if (condition.Type == "r-val")
                        {
                            condition = GetTokenValue(condition);
                        }
                        var result = Convert.ToBoolean(condition.Lexeme);
                        if (!result)
                        {
                            //TODO: add jump to label
                            var token = CodeTable[currentInstructionIndex - 1];
                            var label = LabelTable.Find(x => x.Name == token.Lexeme);
                            currentInstructionIndex = label.Index; //TODO: add +1
                                                                   //Console.WriteLine("false condition");
                            continue;
                        }

                        break;
                    }
                case "jmp":
                    {
                        var token = CodeTable[currentInstructionIndex - 1];
                        var label = LabelTable.Find(x => x.Name == token.Lexeme);
                        currentInstructionIndex = label.Index;
                        continue;
                    }
                case "assign_op":
                    {
                        var value = operationStack.Pop();
                        var variable = operationStack.Pop();
                        PerformAssignOperation(value, variable);
                        break;
                    }
                case "print":
                    {
                        var value = operationStack.Pop();
                        var valueToPrint = GetTokenValue(value);
                        Console.WriteLine($"Вивід: {valueToPrint.Lexeme}");
                        break;
                    }
                case "read":
                    {
                        var variable = operationStack.Pop();
                        var variableValue = GetTokenValue(variable);
                        Console.Write($"Ввведіть значення для змінної: {variable.Lexeme} типу: {variableValue.Lexeme} >>");
                        var result = Console.ReadLine();
                        var varIndex = GlobalVars.VariableTable.FindIndex(x => x.Name == variable.Lexeme);
                        var updatedVariable = GlobalVars.VariableTable[varIndex];
                        //var convertedResult= right.Type == "intnum" ? int.Parse(right.Lexeme) + int.Parse(left.Lexeme) : double.Parse(right.Lexeme) + double.Parse(left.Lexeme);
                        var convertedResult = variableValue.Type == "intnum" ? int.Parse(result) : double.Parse(result);
                        updatedVariable.Value = convertedResult.ToString();
                        GlobalVars.VariableTable[varIndex] = updatedVariable;
                        break;
                    }
            }

            currentInstructionIndex++;
        }
    }

    /// <summary>
    /// Трансляція постфіксного виразу в MSIL
    /// для подальшого виконання на CLR
    /// </summary>
    public string TranslatePostfixToMSIL()
    {
        var msilCode = new StringBuilder();
        msilCode.AppendLine(".assembly program {}");
        msilCode.AppendLine(".method static void Main() cil managed");
        msilCode.AppendLine("{");
        msilCode.AppendLine(".entrypoint");
        // Це для того, щоб вказати максимальну кількість елементів в стеку,
        //TODO: поцікавитись, чи можна автоматизувати
        msilCode.AppendLine(".maxstack 128");
        msilCode.AppendLine(".locals init (");

        //боже мій, вау
        foreach (var (variable, i) in VariableTable.Select((variable, i) => (variable, i)))
        {
            var type = variable.Type switch
            {
                "int" => "int32",
                "double" => "float64",
                "bool" => "bool",
                _ => throw new Exception("Invalid type")
            };
            // add logic to check if it is the last element, if not add comma
            //TODO: виправити це
            if (i != VariableTable.Count - 1)
            {
                msilCode.AppendLine($"[{i}] {type} {variable.Name},");
            }
            else
            {
                msilCode.AppendLine($"[{i}] {type} {variable.Name}");
            }
        }
        msilCode.AppendLine(")");
        int index = 0;
        //додаємо інструкції
        msilCode.Append(GlobalVars.MSILOutput.ToString());
        msilCode.AppendLine("ret");
        msilCode.AppendLine("}");

        return msilCode.ToString();
    }

    private void PerformAssignOperation(Token value, Token variable)
    {
        var valueToAssign = GetValueFromToken(value);
        var variableValue = GetTokenValue(variable);
        if (value.Type is "realnum" && variableValue.Type is "intnum")
        {
            valueToAssign = (int)valueToAssign;
        }

        //Розширення логіки, через int та double
        Console.WriteLine($"Присвоєння: {variable.Lexeme} = {valueToAssign}");
        var varIndex = VariableTable.FindIndex(x => x.Name == variable.Lexeme);
        var updatedVariable = VariableTable[varIndex];
        updatedVariable.Value = valueToAssign.ToString();
        VariableTable[varIndex] = updatedVariable;
    }


    /// <summary>
    /// Метод для виконання уніфікованих операцій порівнянь
    /// Для bool можливо тільки == та !=
    /// Інакше отримаєш Exception
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="operation"></param>
    private Token PerformRelOp(Token left, Token right, OperationType operation)
    {
        bool result = false;
        //Менше зло, можливо передавати відрізняти типи
        var leftValue = GetValueFromToken(left);
        var rightValue = GetValueFromToken(right);

        //TODO: питання, а чи треба для bool, обмежувати операції
        result = operation switch
        {
            OperationType.Equal => leftValue == rightValue,
            OperationType.NotEqual => leftValue != rightValue,
            OperationType.GreaterThan => leftValue > rightValue,
            OperationType.LessThan => leftValue < rightValue,
            OperationType.GreaterThanOrEqual => leftValue >= rightValue,
            OperationType.LessThanOrEqual => leftValue <= rightValue,
            _ => throw new Exception("Invalid operation")
        };

        return new Token(0, result.ToString(), "bool");
    }

    /// <summary>
    /// Метод для отримання значення з токену,
    /// Звичайно, це погана практика, використання dynamic
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private dynamic GetValueFromToken(Token token)
    {
        return token.Type switch
        {
            "intnum" or "int " => int.Parse(token.Lexeme), //TODO:а чому відразу не уніфікувати
            "realnum" or "double" => double.Parse(token.Lexeme),
            "boolval" => bool.Parse(token.Lexeme),
            _ => throw new Exception("Invalid type")
        };
    }

    /// <summary>
    /// Метод для отримання значення змінної
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Повертає об'єкт Token, із lexeme - значенням, type - типом змінної</returns>
    private Token GetTokenValue(Token token)
    {
        if (token.Type is "l-val")
        {
            var variable = VariableTable.FirstOrDefault(x => x.Name == token.Lexeme);
            var typeOfId = variable.Type;
            typeOfId = typeOfId switch
            {
                "int" => "intnum",
                "real" => "realnum",
                "bool" => "bool",
                _ => typeOfId
            };

            return new Token(0, variable.Value, typeOfId);
        }

        if (token.Type is not "r-val") return token;
        {
            var variable = VariableTable.FirstOrDefault(x => x.Name == token.Lexeme);
            var typeOfId = variable.Type;
            typeOfId = typeOfId switch
            {
                "int" => "intnum",
                "real" => "realnum",
                "bool" => "bool",
                _ => typeOfId
            };

            return new Token(0, variable.Value, typeOfId);
        }
    }
}