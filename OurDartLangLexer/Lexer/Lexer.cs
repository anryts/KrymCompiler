using System.Text;
using OurDartLangLexer.Extensions;

namespace OurDartLangLexer.Lexer;

public class Lexer
{
    private Dictionary<(int, string), int> stateTransitions;
    private Dictionary<int, string> finalStateTokens;
    private Dictionary<string, string> keywordTokens;
    private HashSet<int> finalStates;
    private HashSet<int> errorStates;

    public List<(string lexeme, string tokenType)> TokenData { get; private set; } // Stores token data

    public Lexer()
    {
        // Initialize the configurations from LexerConfiguration class
        stateTransitions = LexerConfiguration.GetStateTransitions();
        finalStateTokens = LexerConfiguration.GetFinalStateTokens();
        keywordTokens = LexerConfiguration.GetKeywordTokens();
        finalStates = LexerConfiguration.GetFinalStates();
        errorStates = LexerConfiguration.GetErrorStates();

        TokenData = new List<(string lexeme, string tokenType)>();
    }

    public void ProcessInput(string input)
    {
        int state = 0;
        StringBuilder lexeme = new StringBuilder();

        //ігноруємо коментарі '//'
        // if (input[0] == '/' && input[1] == '/') return;

        foreach (char ch in input)
        {
            string charClass = ch.ClassOfChar();
            var key = (state, charClass);

            if (stateTransitions.TryGetValue(key, out var transition))
            {
                state = transition;
                lexeme.Append(ch);
            }
            else
            {
                if (finalStates.Contains(state))
                {
                    string tokenType = GetTokenType(state, lexeme.ToString());
                    TokenData.Add((lexeme.ToString(), tokenType));
                    lexeme.Clear();
                    state = 0;
                }
                else if (errorStates.Contains(state))
                {
                    TokenData.Add((lexeme.ToString(), "error"));
                    lexeme.Clear();
                    state = 0;
                }
            }
        }
    }


    private string GetTokenType(int state, string lexeme)
    {
        if (finalStateTokens.ContainsKey(state))
        {
            return finalStateTokens[state];
        }
        if (keywordTokens.ContainsKey(lexeme))
        {
            return keywordTokens[lexeme];
        }
        return "unknown";
    }
}