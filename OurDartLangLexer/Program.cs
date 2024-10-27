using OurDartLangLexer.DataProviders;
using OurDartLangLexer.Lexer;


string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
string fileName = "test.our_dart_lang";
string filePath = Path.Combine(projectDirectory, fileName);

if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

var lexer = new Lexer();

var sourceCodeArr = File.ReadAllText(filePath);

lexer.ProcessInput(sourceCodeArr);

OutputHandler.WriteToConsole(lexer.TokenTable, lexer.SymbolTable);
OutputHandler.WriteToFile(Path.Combine(projectDirectory, "output.txt"), lexer.TokenTable, lexer.SymbolTable);