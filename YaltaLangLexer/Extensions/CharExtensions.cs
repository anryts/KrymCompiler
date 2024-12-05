namespace YaltaLangLexer.Extensions;

public static class CharExtensions
{
    /// <summary>
    /// Метод розширення для класифікації символів
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
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