// Start the lexer process

using OurDartLangLexer;
using OurDartLangLexer.DataProviders;
using OurDartLangLexer.Lexer;

//read from input and divide it into lines
string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
string fileName = "test.our_dart_lang";
string filePath = Path.Combine(projectDirectory, fileName);
Lexer lexer = new Lexer();

if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

string[] sourceCodeArr = File.ReadAllLines(filePath);
string sourceCode = "";

foreach (string sourceCodeLine in sourceCodeArr)
{
    lexer.ProcessInput(sourceCodeLine);
}

var outputHandler = new OutputHandler();
outputHandler.WriteToConsole(lexer.TokenData);
outputHandler.WriteToFile("lexer_output.txt", lexer.TokenData);



// Output results to CLI and file
