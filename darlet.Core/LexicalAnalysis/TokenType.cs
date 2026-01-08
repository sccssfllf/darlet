namespace darlet.Core.LexicalAnalysis
{
    /// <summary>
    /// Перелік усіх можливих типів токенів, які розпізнає лексер мови Dartlet.
    /// Використовується парсером для прийняття рішень щодо граматичної структури.
    /// </summary>
    public enum TokenType
    {
        // --- Спеціальні токени управління ---

        /// <summary>Кінець вхідного потоку даних (End Of File).</summary>
        EOF,
        /// <summary>Токен, який не вдалося розпізнати (лексична помилка).</summary>
        UNKNOWN,

        // --- Ідентифікатори та Літерали (Дані) ---

        /// <summary>Ім'я змінної (наприклад: x, count, myVar).</summary>
        IDENTIFIER,
        /// <summary>Ціле число (наприклад: 42).</summary>
        INTEGER_LITERAL,
        /// <summary>Дійсне число (наприклад: 3.14).</summary>
        REAL_LITERAL,
        /// <summary>Булеве значення (true, false).</summary>
        BOOLEAN_LITERAL,
        /// <summary>Рядок тексту в лапках (наприклад: "Hello").</summary>
        STRING_LITERAL,

        // --- Ключові слова (Keywords) ---

        KW_VAR,      // Оголошення змінної
        KW_IF,       // Умовний оператор
        KW_ELSE,     // Альтернативна гілка
        KW_WHILE,    // Цикл
        KW_PRINT,    // Вивід у консоль
        KW_RETURN,   // Повернення значення (зарезервовано)
        KW_INPUT,    // Ввід з консолі

        // --- Роздільники (Delimiters) ---

        LPAREN,      // (
        RPAREN,      // )
        LBRACE,      // {
        RBRACE,      // }
        SEMICOLON,   // ; (кінець інструкції)

        // --- Арифметичні оператори ---

        OP_PLUS,     // +
        OP_MINUS,    // -
        OP_MULTIPLY, // *
        OP_DIVIDE,   // /
        OP_POWER,    // ^ (піднесення до степеня)

        // --- Оператор присвоєння ---

        OP_ASSIGN,   // =

        // --- Оператори порівняння (Boolean Logic) ---

        OP_EQUAL,         // ==
        OP_NOT_EQUAL,     // !=
        OP_LESS,          // <
        OP_GREATER,       // >
        OP_LESS_EQUAL,    // <=
        OP_GREATER_EQUAL  // >=
    }
}