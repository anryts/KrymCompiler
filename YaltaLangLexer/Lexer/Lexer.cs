using System.Text;
using YaltaLangLexer.Extensions;
using YaltaLangLexer.Lexer;

namespace OurDartLangLexer.Lexer;

public class Lexer
{
    private readonly Dictionary<(int, string), int> _stateTransitions = LexerConfiguration.GetStateTransitions();
    private readonly Dictionary<int, string> _finalStateTokens = LexerConfiguration.GetFinalStateTokens();
    private readonly Dictionary<string, string> _keywordTokens = LexerConfiguration.GetKeywordTokens();
    private readonly HashSet<int> _finalStates = LexerConfiguration.GetFinalStates();
    private readonly HashSet<int> _errorStates = LexerConfiguration.GetErrorStates();
    private int _numLine = 1;
    private int _numChar = -1; // Нумерація з 0
    public List<Token> TokenTable { get; set; } = new();
    public SymbolTable SymbolTable { get; set; } = new();
    public List<string> ErrorTable { get; set; } = new();

    // Initialize the configurations from LexerConfiguration class

    /// <summary>
    /// Початок лексичного аналізу
    /// </summary>
    /// <param name="input">зчитаний текст програми</param>
    public void ProcessInput(string input)
    {
        int state = 0;
        var lexeme = new StringBuilder();

        try
        {
            while (_numChar < input.Length)
            {
                if (_numChar + 1 >= input.Length) break;

                char charSymbol = input[++_numChar];
                string classCh = charSymbol.ClassOfChar();
                state = NextState(state, classCh);

                // Перевірка на помилкові стани йде першою, щоб відразу очистити лексему
                if (_errorStates.Contains(state))
                {
                    ProcessError(ref state, lexeme);
                }
                else if (_finalStates.Contains(state))
                {
                    Processing(ref state, lexeme, charSymbol.ToString());
                }
                else
                {
                    if (charSymbol == ' ' || charSymbol == '\n' || charSymbol == '\t')
                    {
                        continue;
                    }

                    lexeme.Append(charSymbol);
                }
            }

            Console.WriteLine("Lexer: Лексичний аналіз завершено успішно");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lexer: Аварійне завершення програми з кодом {e.Message}");
        }
    }

    private void ProcessError(ref int state, StringBuilder lexeme)
    {
        if (state == 100)
        {
            ErrorTable.Add($"{_numLine}: невідомий символ {lexeme}");
        }

        if (state == 109)
        {
            ErrorTable.Add($"{_numLine}: Очікувався символ =, але знайдено {lexeme}");
        }

        TokenTable.Add(new Token(_numLine, lexeme.ToString(), "error"));
        lexeme.Clear();
        state = 0;
    }

    private int NextState(int state, string classCh)
    {
        try
        {
            state = _stateTransitions[(state, classCh)];
        }
        catch (Exception e)
        {
            state = _stateTransitions[(state, "other")];
        }

        return state;
    }

    /// <summary>
    /// Аналізуємо лексему в залежності від стану
    /// </summary>
    /// <param name="state">Поточний стан, передається як ref</param>
    /// <param name="lexeme">Поточний лексема</param>
    /// <param name="token">Поточний символ</param>
    private void Processing(ref int state, StringBuilder lexeme, string token)
    {
        switch (state)
        {
            case 12 or 22 or 24:
                ProcessKeywordOrIdentifierOrNumber(ref state, lexeme, token);
                break;
            case 31 or 63 or 93:
                ProcessOperatorOrSymbol(ref state, lexeme);
                break;
            case 32 or 41 or 71 or 81 or 92:
                ProcessCompoundOperatorOrSymbol(ref state, lexeme, token);
                break;
            case 51:
                ProcessEndOfLine(ref state);
                break;
            case 62:
                ProcessComment(ref state);
                break;
            default:
                break;
        }
    }

    private string GetTokenType(int state, string lexeme)
        => _keywordTokens.ContainsKey(lexeme)
            ? _keywordTokens[lexeme]
            : _finalStateTokens[state];


    private void ProcessComment(ref int state)
    {
        state = 0;
        _numLine++;
    }

    private void ProcessEndOfLine(ref int state)
    {
        _numLine++;
        state = 0;
    }

    private void ProcessCompoundOperatorOrSymbol(ref int state, StringBuilder lexeme, string token)
    {
        lexeme.Append(token);
        string tokenType = GetTokenType(state, lexeme.ToString());
        TokenTable.Add(new Token(_numLine, lexeme.ToString(), tokenType));
        lexeme.Clear();
        state = 0;
    }

    private void ProcessOperatorOrSymbol(ref int state, StringBuilder lexeme)
    {
        var tokenType = GetTokenType(state, lexeme.ToString());
        TokenTable.Add(new Token(_numLine, lexeme.ToString(), tokenType));
        lexeme.Clear();
        state = 0;
        _numChar = PutCharBack(_numChar); //зірочка
    }

    private void ProcessKeywordOrIdentifierOrNumber(ref int state, StringBuilder lexeme, string token)
    {
        var tokenType = GetTokenType(state, lexeme.ToString());
        switch (tokenType)
        {
            case "keyword":
            {
                int indx = IndexIdConst(state, lexeme.ToString(), tokenType);
                TokenTable.Add(
                    new Token(_numLine, lexeme.ToString(), tokenType, indx));
                break;
            }
            case "identifier":
            {
                int indx = IndexIdConst(state, lexeme.ToString(), tokenType);
                TokenTable.Add(new Token(_numLine, lexeme.ToString(), tokenType, indx));
                break;
            }
            case "intnum" or "realnum" or "boolval":
            {
                int indx = IndexIdConst(state, lexeme.ToString(), tokenType);
                TokenTable.Add(new Token(_numLine, lexeme.ToString(), tokenType, indx));
                break;
            }
            case "id":
            {
                int indx = IndexIdConst(state, lexeme.ToString(), tokenType);
                TokenTable.Add(new Token(_numLine, lexeme.ToString(), tokenType, indx));
                break;
            }
        }

        lexeme.Clear();
        state = 0;
        _numChar = PutCharBack(_numChar); //зірочка
    }

    private int IndexIdConst(int state, string lexeme, string tokenType)
    {
        if (state == 12)
        {
            return SymbolTable.GetOrAddIdentifier(lexeme);
        }

        if (state == 22 || state == 24)
        {
            return SymbolTable.GetOrAddConstant(lexeme,
                tokenType);
        }

        throw new Exception($"Невідомий тип лексеми {_numLine} рядок, {lexeme} лексема");
    }

    private Token FindToken(string lexeme)
        => TokenTable.Find(token => token.Lexeme == lexeme);

    private int PutCharBack(int numberOfChar)
        => numberOfChar - 1;
}