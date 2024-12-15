using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaltaLangParser;

namespace YaltaLangMachine;

public class FileWriter (Parser parser)
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

            //TODO Constants add 
            //sw.WriteLine(".constants(");
            //foreach (var constant in parser._lexer.)
            //{
            //    writer.WriteLine($"   {constant.Key,-6} {constant.Value,-10}");
            //}
            //writer.WriteLine(")\n");

            // Code
            sw.WriteLine(".code(");
            foreach (var item in parser.CodeTable)
            {
                sw.WriteLine($"   {item.Lexeme,-6} {item.Type,-6}");
            }
            sw.WriteLine(")\n");
        }
    }
}