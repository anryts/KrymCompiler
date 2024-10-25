namespace OurDartLangLexer.Lexer;

public struct Token(string lexeme, string type)
{
    private string Lexeme { get; set; } = lexeme;
    private string Type { get; set; } = type;


    public override string ToString()
    {
        return $"{Lexeme,-10} {Type,-10}";
    }
}