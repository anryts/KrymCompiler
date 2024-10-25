using System.Text;
using OurDartLangLexer.Extensions;

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
    public List<Token> TokenTable { get; set; } = new List<Token>();

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

                if (_finalStates.Contains(state))
                {
                    Processing(ref state, lexeme, charSymbol.ToString());
                }
                else if (_errorStates.Contains(state))
                {
                    TokenTable.Add(new Token(lexeme.ToString(), "error"));
                    lexeme.Clear();
                    state = 0;
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
    /// <param name="input">Поточний символ</param>
    private void Processing(ref int state, StringBuilder lexeme, string input)
    {
        switch (state)
        {
            case 12 or 22 or 24:
                ProcessKeywordOrIdentifierOrNumber(ref state, lexeme, input);
                break;
            case 31 or 63 or 93:
                ProcessOperatorOrSymbol(ref state, lexeme);
                break;
            case 32 or 41 or 71 or 81 or 92:
                ProcessCompoundOperatorOrSymbol(ref state, lexeme, input);
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
        => state = 0;

    private void ProcessEndOfLine(ref int state)
    {
        _numLine++;
        state = 0;
    }

    private void ProcessCompoundOperatorOrSymbol(ref int state, StringBuilder lexeme, string input)
    {
        lexeme.Append(input);
        string tokenType = GetTokenType(state, lexeme.ToString());
        TokenTable.Add(new Token(lexeme.ToString(), tokenType));
        lexeme.Clear();
        state = 0;
    }

    private void ProcessOperatorOrSymbol(ref int state, StringBuilder lexeme)
    {
        var tokenType = GetTokenType(state, lexeme.ToString());
        TokenTable.Add(new Token(lexeme.ToString(), tokenType));
        lexeme.Clear();
        state = 0;
        _numChar = PutCharBack(_numChar);
    }

    private void ProcessKeywordOrIdentifierOrNumber(ref int state, StringBuilder lexeme, string token)
    {
        var tokenType = GetTokenType(state, lexeme.ToString());
        TokenTable.Add(new Token(lexeme.ToString(), tokenType));
        lexeme.Clear();
        state = 0;
        _numChar = PutCharBack(_numChar);
    }

    private int PutCharBack(int numberOfChar)
        => numberOfChar - 1;
}