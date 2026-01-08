namespace darlet.Core.LexicalAnalysis;

public enum TokenType
{
    // Управління
    EOF,                // Кінець файлу (End of File)
    UNKNOWN,            // Невідома помилка (Unknown error)
    
    // Ідентифікатори та літерали
    IDENTIFIER,         // Name of variables 
    INTEGER_LITERAL,    // Integers
    REAL_LITERAL,       // Real numbers
    BOOLEAN_LITERAL,    // True, False
    STRING_LITERAL,     // "Hello World!"

    // Ключові слова
    KW_VAR,             // var
    KW_IF,              // if
    KW_ELSE,            // else
    KW_WHILE,           // while
    KW_PRINT,           // print
    KW_RETURN,          // return
    KW_INPUT,           // input

    // Роздільники
    LPAREN,             // (
    RPAREN,             // )
    LBRACE,             // {
    RBRACE,             // }
    SEMICOLON,          // ;

    // Оператори
    OP_PLUS,            // +
    OP_MINUS,           // -
    OP_MULTIPLY,        // *
    OP_DIVIDE,          // /
    OP_POWER,           // ^

    // Присвоєння
    OP_ASSIGN,          // =

    // Порівняння
    OP_EQUAL,           // ==
    OP_NOT_EQUAL,       // !=
    OP_LESS,            // <
    OP_GREATER,         // >
    OP_LESS_EQUAL,      // <=
    OP_GREATER_EQUAL,   // >=

}
