namespace OurDartLangLexer;

public class Token(string lexeme, string type)
{
    public string Lexeme { get; set; } = lexeme;
    public string Type { get; set; } = type;

    public override string ToString()
    {
        return $"{Lexeme,-10} {Type,-10}";
    }
}