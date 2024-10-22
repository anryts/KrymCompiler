namespace OurDartLangLexer;

public class SymbolTable
{
    public Dictionary<string, int> Identifiers { get; } = new Dictionary<string, int>();
    public Dictionary<string, Tuple<string, int>> Constants { get; } = new Dictionary<string, Tuple<string, int>>();

    public int GetOrAddIdentifier(string lexeme)
    {
        if (!Identifiers.ContainsKey(lexeme))
        {
            int index = Identifiers.Count + 1;
            Identifiers[lexeme] = index;
        }
        return Identifiers[lexeme];
    }

    public int GetOrAddConstant(string lexeme, string type)
    {
        if (!Constants.ContainsKey(lexeme))
        {
            int index = Constants.Count + 1;
            Constants[lexeme] = new Tuple<string, int>(type, index);
        }
        return Constants[lexeme].Item2;
    }
}