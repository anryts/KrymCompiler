using System.Text;
using Common;
using Common.Extensions;
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

    //TODO: make a result object
    public string ErroMessage { get; set; }

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
                    int charPosition = FindCurrentPositionRelativeToCurrentLine(input, _numChar);
                    ProcessError(ref state, lexeme, charSymbol.ToString(), charPosition);
                }
                else if (_finalStates.Contains(state))
                {
                    Processing(ref state, lexeme, charSymbol.ToString(), input);
                }
                else
                {
                    if (charSymbol == ' '|| charSymbol == '\r' || charSymbol == '\n' || charSymbol == '\t')
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
            ErroMessage = e.Message;
        }
    }

    private void ProcessError(ref int state, StringBuilder lexeme, string charSymbol, int charPosition)
    {
        if (state == 100)
        {
            ErrorTable.Add($"{_numLine}: невідомий символ {charSymbol} на позиції {charPosition}");
        }

        if (state == 109)
        {
            ErrorTable.Add($"{_numLine}: Очікувався символ =, але знайдено {lexeme}");
        }

        var token = lexeme.ToString();
        TokenTable.Add(new Token(_numLine, token.Length == 0 ? charSymbol : token, "error"));
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
    private void Processing(ref int state, StringBuilder lexeme, string token, string input)
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
                ProcessEndOfLine(ref state, input);
                break;
            case 62:
                ProcessComment(ref state, lexeme);
                break;
            default:
                break;
        }
    }

    private string GetTokenType(int state, string lexeme)
        => _keywordTokens.ContainsKey(lexeme)
            ? _keywordTokens[lexeme]
            : _finalStateTokens[state];


    private void ProcessComment(ref int state, StringBuilder lexeme)
    {
        state = 0;
        _numChar = PutCharBack(_numChar); //зірочка
        lexeme.Clear();
        _numLine++;
    }

    private void ProcessEndOfLine(ref int state, string input)
    {
        //\r\n на Windows розпізнається як окремий символ (потужно дякуємо),
        //через що маємо проблеми із дублюванням
        //Як варіант, оновити правила, але мені зараз це робити ліньки
        // Якщо поточний символ відповідає Environment.NewLine
        //TODO: перевір, як воно буде працювати на Unix, можливо, потрібно буде змінити ...
        if (_numChar + Environment.NewLine.Length - 1 < input.Length &&
            input.Substring(_numChar, Environment.NewLine.Length) == Environment.NewLine)
        {
            _numChar += Environment.NewLine.Length - 1; // Пропускаємо символи нового рядка
        }

        _numLine++; // Збільшуємо номер рядка
        state = 0;  // Повертаємо стан автомата у початковий
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
                    TokenTable.Add(
                        new Token(_numLine, lexeme.ToString(), tokenType));
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

    private int FindCurrentPositionRelativeToCurrentLine(string input, int numChar)
    {
        int position = 0;
        for (int i = 0; i < numChar; i++)
        {
            if (input[i] == '\n')
            {
                position = 0;
            }
            else
            {
                position++;
            }
        }

        return ++position;
    }
}