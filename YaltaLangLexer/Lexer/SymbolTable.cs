namespace YaltaLangLexer.Lexer;

public class SymbolTable
{
    public Dictionary<string, int> Identifiers { get; } = new();
    public Dictionary<string, Tuple<string, int>> Constants { get; } = new();

    public int GetOrAddIdentifier(string lexeme)
    {
        if (!Identifiers.ContainsKey(lexeme))
        {
            int index = Identifiers.Count + 1;
            Identifiers.Add(lexeme, index);
        }
        return Identifiers[lexeme];
    }

    public int GetOrAddConstant(string lexeme, string type)
    {
        if (!Constants.ContainsKey(lexeme))
        {
            int index = Constants.Count + 1;
            Constants.Add(lexeme, new Tuple<string, int>(type, index));
        }
        return Constants[lexeme].Item2;
    }
}
