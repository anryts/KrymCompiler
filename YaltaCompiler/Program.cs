using OurDartLangLexer.Lexer;
using System.Globalization;
using YaltaLangLexer.DataProviders;
using YaltaLangMachine;
using YaltaLangParser;


// Set the culture to invariant globally
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;


string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
string fileName = "test.yt";
string filePath = Path.Combine(projectDirectory, "ExamplesOfCode", "ForParserTests", fileName);

if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

var lexer = new Lexer();

var sourceCodeArr = File.ReadAllText(filePath);

lexer.ProcessInput(sourceCodeArr);
OutputHandler.WriteToConsole(lexer.TokenTable, lexer.SymbolTable, lexer.ErrorTable);

var parser = new Parser(lexer);

parser.ParseProgram();

var translator = new PSM(parser);
translator.ParsePostfixProgram();
translator.WriteToFile("test");

//TODO: test a parser for numeric expressions