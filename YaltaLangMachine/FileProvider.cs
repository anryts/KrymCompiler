using Common;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using YaltaLangParser;

namespace YaltaLangMachine;

public class FileProvider(Parser parser)
{
    public void WriteToFile(string fileName = "default")
    {
        //шлях до файлу
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "PostfixCode", fileName);
        fileName = filePath + ".postfix";
        using (StreamWriter sw = new StreamWriter(fileName, false))
        {
            // Header
            sw.WriteLine(".target: Postfix Machine");
            sw.WriteLine(".version: 1a\n");

            sw.WriteLine(".variables(");
            foreach (var item in GlobalVars.VariableTable)
            {
                sw.WriteLine($"   {item.Name,-6} {item.Type,-6} {item.Value,-6}");
            }
            sw.WriteLine(")\n");

            sw.WriteLine(".labels(");
            foreach (var label in parser.LabelTable)
            {
                sw.WriteLine($"   {label.Name,-6} {label.Index,-6}");
            }
            sw.WriteLine(")\n");

            sw.WriteLine(".constants(");
            foreach (var constant in parser._lexer.TokenTable)
            {
                if (constant.Type == "intnum" || constant.Type == "realnum" || constant.Type == "boolval")
                {
                    sw.WriteLine($"   {constant.Lexeme,-6} {constant.Type,-6}");
                }
            }
            sw.WriteLine(")\n");

            // Code
            sw.WriteLine(".code(");
            foreach (var item in parser.CodeTable)
            {
                sw.WriteLine($"   {item.Lexeme,-6} {item.Type,-6}");
            }
            sw.WriteLine(")\n");
        }
    }

    /// <summary>
    /// Повертає кортеж (таблицю змінних, таблицю міток, таблицю коду)
    /// Так, це злочин, але ж його ніхто не побачить
    /// </summary>
    /// <param name="fileName"></param>
    public (List<Variable> variableTable, List<Label> labelTable, List<Token> codeTable) ReadFromFile(string fileName = "default")
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "PostfixCode", fileName);
        fileName = filePath + ".postfix";

        List<Variable> VariableTable = new List<Variable>();
        List<Label> LabelTable = new List<Label>();
        List<Constant> ConstantTable = new List<Constant>();
        List<Token> CodeTable = new List<Token>();

        string section = "";
        string[] lines = File.ReadAllLines(fileName);

        foreach (var line in lines)
        {
            // Перевіряємо розділ за заголовками
            if (line.StartsWith(".variables")) { section = "variables"; continue; }
            if (line.StartsWith(".labels")) { section = "labels"; continue; }
            if (line.StartsWith(".constants")) { section = "constants"; continue; }
            if (line.StartsWith(".code")) { section = "code"; continue; }

            // Завершення секції
            if (line.StartsWith(")")) { section = ""; continue; }

            // Обробка відповідного розділу
            if (section == "variables")
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    VariableTable.Add(new Variable(parts[0], parts[1], string.Empty));
                }
            }
            else if (section == "labels")
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    LabelTable.Add(new Label(parts[0], int.Parse(parts[1])));
                }
            }
            else if (section == "code")
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                CodeTable.Add(new Token(0, parts[0], parts[1]));
            }
        }
        return (VariableTable, LabelTable, CodeTable);
    }
}