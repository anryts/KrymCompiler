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

var lexerResult = lexer.ProcessInput(sourceCodeArr);
if (lexerResult == 1)
{
    return;
}
OutputHandler.WriteToConsole(lexer.TokenTable, lexer.SymbolTable, lexer.ErrorTable);

var parser = new Parser(lexer);

var parserResult = parser.ParseProgram();
if (parserResult == 1)
{
    return;
}

var fileProvider= new FileProvider(parser);
fileProvider.WriteToFile("test");
var result = fileProvider.ReadFromFile("test");
var translator = new PSM(result.labelTable, result.variableTable, result.codeTable);

translator.ParsePostfixProgram();
var msilCode = translator.TranslatePostfixToMSIL();

var msilFilePath = Path.Combine(projectDirectory, "ExamplesOfCode", "ForParserTests", "test.il");
File.WriteAllText(msilFilePath, msilCode);

