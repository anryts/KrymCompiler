namespace OurDartLangLexer.Extensions;

public static class CharExtensions
{
    public static string ClassOfChar(this char ch)
    {
        return ch switch
        {
            '.' => "dot",
            ' ' or '\t' => "ws",
            '\n' or '\r' => "eol",
            _ when char.IsLower(ch) => "Letter",
            _ when char.IsDigit(ch) => "Digit",
            _ when "=+-*/^<>!(){};".Contains(ch) => ch.ToString(),
            _ => "other"
        };
    }
}