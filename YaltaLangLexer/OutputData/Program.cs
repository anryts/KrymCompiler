using OurDartLangLexer.Lexer;
using YaltaLangLexer.DataProviders;


string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
string fileName = "nested.yt";
string filePath = Path.Combine(projectDirectory, "ExamplesOfCode", fileName);

if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

var lexer = new Lexer();

var sourceCodeArr = File.ReadAllText(filePath);

lexer.ProcessInput(sourceCodeArr);

OutputHandler.WriteToConsole(lexer.TokenTable, lexer.SymbolTable, lexer.ErrorTable);