namespace YaltaLangLexer.Extensions;

public enum TokenType
{
    Letter,         // Represents any letter (a-z, A-Z)
    Digit,          // Represents any digit (0-9)
    Other,          // Represents any other character not explicitly listed
    Dot,            // Represents '.' (dot/period)
    Assign,         // Represents '='
    Semicolon,      // Represents ';'
    EndOfLine,      // Represents end of line or newline character
    Slash,          // Represents '/'
    OpenParen,      // Represents '('
    CloseParen,     // Represents ')'
    OpenBrace,      // Represents '{'
    CloseBrace,     // Represents '}'
    Asterisk,       // Represents '*'
    Caret,          // Represents '^'
    Plus,           // Represents '+'
    Minus,          // Represents '-'
    GreaterThan,    // Represents '>'
    LessThan,       // Represents '<'
    Exclamation,    // Represents '!'
    Whitespace,      // Represents whitespace (space, tab, etc.)

    // Add new token types here
    Id,
    IntNum,
    RealNum,
    Comment,

    // Add new token types here
    Boolean,
    Keyword,

}