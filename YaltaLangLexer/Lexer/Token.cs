namespace YaltaLangLexer.Lexer;

public struct Token(int numLine, string lexeme, string type, int? index = null)
{
    public string Lexeme { get; set; } = lexeme;
    public string Type { get; set; } = type;
    public string? Index { get; set; } = index.ToString();
    public string NumLine { get; set; } = numLine.ToString();

    public override string ToString()
    {
        return $"{NumLine, -3} {Lexeme,-10} {Type,-10}, {Index: -5}";
    }
}