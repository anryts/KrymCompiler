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
        foreach (var item in parser.CodeTable)
        {
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
                case "assign_op":
                    {
                        var value = operationStack.Pop();
                        var variable = operationStack.Pop();
                        Console.WriteLine($"Присвоєння: {variable.Lexeme} = {value.Lexeme}");
                        break;
                    }
            }
        }
    }

    public void WriteToFile(string fileName = "default")
    {
        FileWriter.WriteToFile(fileName);
    }
}
