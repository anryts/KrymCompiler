using Common;
using OurDartLangLexer.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaltaLangParser;

namespace YaltaLangMachine;

public class PSM(Parser parser)
{

    public FileWriter FileWriter { get; set; } = new FileWriter(parser);
    public void ParsePostfixProgram()
    {
        //GOD method in action
        var operationStack = new Stack<Token>();
        int currentInstructionIndex = 0;
        while (currentInstructionIndex < parser.CodeTable.Count)
        {
            var item = parser.CodeTable[currentInstructionIndex];
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
                        var result = right.Type == "intnum" ? -int.Parse(right.Lexeme) : -double.Parse(right.Lexeme);
                        operationStack.Push(new Token(0, result.ToString(), right.Type));
                        break;
                    }
                case "intnum" or "realnum":
                    {
                        operationStack.Push(item);
                        break;
                    }
                case "rel_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();
                        //TODO: check for && and || operators
                        // TODO, add real var assignment
                        if (right.Type == "r-val")
                        {
                            var typeOfId = GlobalVars.VariableTable.FirstOrDefault(x => x.Name == right.Lexeme).Type;
                            if (typeOfId is "int")
                            {
                                typeOfId = "intnum";
                            }
                            else if (typeOfId is "real")
                            {
                                typeOfId = "realnum";
                            }
                            else if (typeOfId is "bool")
                            {
                                typeOfId = "bool";
                            }
                            right = new Token(0, GlobalVars.VariableTable.Find(x => x.Name == right.Lexeme).Value, right.Type);
                        }
                        if (left.Type == "r-val")
                        {
                            var typeOfId = GlobalVars.VariableTable.FirstOrDefault(x => x.Name == left.Lexeme).Type;
                            if (typeOfId is "int")
                            {
                                typeOfId = "intnum";
                            }
                            else if (typeOfId is "real")
                            {
                                typeOfId = "realnum";
                            }
                            else if (typeOfId is "bool")
                            {
                                typeOfId = "bool";
                            }
                            var variable = GlobalVars.VariableTable.Find(x => x.Name == left.Lexeme);
                            left = new Token(0, variable.Value, typeOfId);
                        }

                        switch (item.Lexeme)
                        {
                            case "==":
                                {
                                    bool result = false;
                                    if (left.Type == "intnum" && right.Type == "intnum")
                                    {
                                        result = int.Parse(left.Lexeme) == int.Parse(right.Lexeme);
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else if (left.Type == "realnum" && right.Type == "realnum")
                                    {
                                        result = double.Parse(left.Lexeme) == double.Parse(right.Lexeme);
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else if (left.Type == "bool" && right.Type == "bool")
                                    {
                                        result = left.Lexeme == right.Lexeme;
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else
                                    {
                                        operationStack.Push(new Token(0, "false", "bool"));
                                    }

                                    break;
                                }
                            case "!=":
                                {
                                    bool result = false;
                                    if (left.Type == "intnum" && right.Type == "intnum")
                                    {
                                        result = int.Parse(left.Lexeme) != int.Parse(right.Lexeme);
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else if (left.Type == "realnum" && right.Type == "realnum")
                                    {
                                        result = double.Parse(left.Lexeme) != double.Parse(right.Lexeme);
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else if (left.Type == "bool" && right.Type == "bool")
                                    {
                                        result = left.Lexeme != right.Lexeme;
                                        operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    }
                                    else
                                    {
                                        operationStack.Push(new Token(0, "false", "bool"));
                                    }

                                    break;
                                }
                            case "<":
                                {
                                    var result = left.Lexeme == "intnum" ? int.Parse(left.Lexeme) < int.Parse(right.Lexeme) : double.Parse(left.Lexeme) < double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    break;
                                }
                            case ">":
                                {
                                    var result = left.Lexeme == "intnum" ? int.Parse(left.Lexeme) > int.Parse(right.Lexeme) : double.Parse(left.Lexeme) > double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    break;
                                }
                            case "<=":
                                {
                                    var result = left.Lexeme == "intnum" ? int.Parse(left.Lexeme) <= int.Parse(right.Lexeme) : double.Parse(left.Lexeme) <= double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    break;
                                }
                            case ">=":
                                {
                                    var result = left.Lexeme == "intnum" ? int.Parse(left.Lexeme) >= int.Parse(right.Lexeme) : double.Parse(left.Lexeme) >= double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), "bool"));
                                    break;
                                }
                        }
                        break;
                    }
                case "add_op":
                    {
                        var right = operationStack.Pop();
                        var left = operationStack.Pop();
                        switch (item.Lexeme)
                        {
                            case "+":
                                {
                                    var result = right.Type == "intnum" ? int.Parse(right.Lexeme) + int.Parse(left.Lexeme) : double.Parse(right.Lexeme) + double.Parse(left.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), right.Type));
                                    break;
                                }
                            case "-":
                                {
                                    var result = right.Type == "intnum" ? int.Parse(left.Lexeme) - int.Parse(right.Lexeme) : double.Parse(left.Lexeme) - double.Parse(right.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), right.Type));
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
                                    var result = right.Type == "intnum" ? int.Parse(right.Lexeme) * int.Parse(left.Lexeme) : double.Parse(right.Lexeme) * double.Parse(left.Lexeme);
                                    operationStack.Push(new Token(0, result.ToString(), right.Type));
                                    break;
                                }
                            case "/":
                                {
                                    var result = right.Type == "intnum" ? int.Parse(left.Lexeme) / int.Parse(right.Lexeme) : double.Parse(left.Lexeme) / double.Parse(right.Lexeme);
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
                        operationStack.Push(new Token(0, result.ToString(), right.Type));
                        break;
                    }
                case "jf":
                    {
                        var condition = operationStack.Pop();
                        var result = Convert.ToBoolean(condition.Lexeme);
                        if (!result)
                        {
                            //TODO: add jump to label
                            var token = parser.CodeTable[currentInstructionIndex - 1];
                            var label = parser.LabelTable.Find(x => x.Name == token.Lexeme);
                            currentInstructionIndex = label.Index;
                            //Console.WriteLine("false condition");
                            continue;
                        }
                        break;
                    }
                case "jmp":
                    {

                        var token = parser.CodeTable[currentInstructionIndex - 1];
                        var label = parser.LabelTable.Find(x => x.Name == token.Lexeme);
                        currentInstructionIndex = label.Index;
                        break;
                    }
                case "assign_op":
                    {
                        var value = operationStack.Pop();
                        var variable = operationStack.Pop();
                        Console.WriteLine($"Присвоєння: {variable.Lexeme} = {value.Lexeme}");
                        var varIndex = GlobalVars.VariableTable.FindIndex(x => x.Name == variable.Lexeme);
                        var updatedVariable = GlobalVars.VariableTable[varIndex];
                        updatedVariable.Value = value.Lexeme;
                        GlobalVars.VariableTable[varIndex] = updatedVariable;
                        break;
                    }
            }
            currentInstructionIndex++;
        }
    }

    public void WriteToFile(string fileName = "default")
    {
        FileWriter.WriteToFile(fileName);
    }
}
