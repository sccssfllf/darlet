namespace darlet.Core.LexicalAnalysis
{
    /// <summary>
    /// Представляє окрему лексему (токен) — мінімальну значущу одиницю коду.
    /// Це основний об'єкт, яким оперують Лексер і Парсер.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Тип токена (наприклад, KW_VAR, IDENTIFIER, OP_PLUS).
        /// Визначає, як цей токен трактуватиметься граматикою.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Фактичне текстове значення токена, як воно було у вихідному коді.
        /// </summary>
        public string Lexeme { get; }

        /// <summary>
        /// Номер рядка, де було знайдено токен.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Номер колонки (позиція в рядку).
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Створює новий екземпляр токена.
        /// </summary>
        /// <param name="type">Категорія токена.</param>
        /// <param name="lexeme">Рядкове значення.</param>
        /// <param name="line">Рядок у файлі.</param>
        /// <param name="column">Колонка у файлі.</param>
        public Token(TokenType type, string lexeme, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Повертає рядкове представлення токена для налагодження.
        /// Формат: [Type] "Lexeme" (Line:Col)
        /// </summary>
        public override string ToString()
        {
            return $"[{Type}] \"{Lexeme}\" ({Line}:{Column})";
        }
    }
}