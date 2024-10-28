using YaltaLangLexer.Extensions;

namespace YaltaLangLexer.Lexer;

public static class LexerConfiguration
{
    public static Dictionary<(int, string), int> GetStateTransitions()
    {
        return new Dictionary<(int, string), int>
        {
            { (0, "Letter"), 11 },
            { (11, "Letter"), 11 },
            { (11, "Digit"), 11 },
            { (11, "other"), 12 },
            { (0, "Digit"), 21 },
            { (21, "Digit"), 21 },
            { (21, "other"), 22 },
            { (21, "dot"), 23 },
            { (23, "Digit"), 23 },
            { (23, "other"), 24 },
            { (0, "="), 30 },
            { (30, "="), 32 },
            { (30, "other"), 31 },
            { (0, ";"), 41 },
            { (0, "eol"), 51 },
            { (0, "/"), 60 },
            { (60, "/"), 61 },
            { (61, "other"), 61 },
            { (61, "eol"), 62 },
            { (60, "other"), 63 },
            { (0, "("), 71 },
            { (0, ")"), 71 },
            { (0, "{"), 71 },
            { (0, "}"), 71 },
            { (0, "*"), 81 },
            { (0, "^"), 81 },
            { (0, "+"), 81 },
            { (0, "-"), 81 },
            { (0, ">"), 90 },
            { (0, "<"), 90 },
            { (90, "="), 92 },
            { (0, "!"), 91 },
            { (91, "="), 92 },
            { (90, "other"), 93 },
            { (0, "ws"), 0 },
            { (0, "other"), 100 },
            { (91, "other"), 109 }
        };
    }

    public static Dictionary<int, string> GetFinalStateTokens()
    {
        return new Dictionary<int, string>
        {
            { 12, "id" },
            { 22, "intnum" },
            { 24, "realnum" },
            { 62, "comment" }
        };
    }

    public static Dictionary<string, string> GetKeywordTokens()
    {
        return new Dictionary<string, string>
        {
            { "true", "boolval" },
            { "false", "boolval" },
            { "main", "keyword" },
            { "int", "keyword" },
            { "double", "keyword" },
            { "bool", "keyword" },
            { "when", "keyword" },
            { "fallback", "keyword" },
            { "while", "keyword" },
            { "read", "keyword" },
            { "print", "keyword" },
            { "=", "assign_op" },
            { "+", "add_op" },
            { "-", "add_op" },
            { "*", "mult_op" },
            { "/", "mult_op" },
            { "^", "exp_op" },
            { "==", "rel_op" },
            { "!=", "rel_op" },
            { ">", "rel_op" },
            { ">=", "rel_op" },
            { "<", "rel_op" },
            { "<=", "rel_op" },
            { "(", "brackets_op" },
            { ")", "brackets_op" },
            { "{", "braces_op" },
            { "}", "braces_op" },
            { ";", "punct" },
            { "\n", "eol" },
            { "\r\n", "eol" },
            { "\t", "ws" },
            { " ", "ws" }
        };
    }

    public static HashSet<int> GetFinalStates()
    {
        return new HashSet<int> { 12, 22, 24, 31, 32, 41, 51, 62, 63, 71, 81, 92, 93, 100, 109 };
    }

    public static HashSet<int> GetErrorStates()
    {
        return new HashSet<int> { 100, 109 };
    }
}