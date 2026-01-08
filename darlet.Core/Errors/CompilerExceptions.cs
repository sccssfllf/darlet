using System;

namespace darlet.Core.Errors
{
    /// <summary>
    /// Базовий клас винятків для компілятора Darlet.
    /// Зберігає детальну інформацію про місце виникнення помилки у вихідному коді.
    /// </summary>
    public class CompilerException : Exception
    {
        /// <summary>
        /// Номер рядка, де сталася помилка.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Позиція (колонка) в рядку.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Категорія помилки (наприклад, "Lexical Error", "Syntax Error").
        /// </summary>
        public string ErrorType { get; }

        /// <summary>
        /// Ініціалізує новий екземпляр помилки компіляції.
        /// </summary>
        /// <param name="message">Текст повідомлення про помилку.</param>
        /// <param name="line">Номер рядка.</param>
        /// <param name="column">Номер колонки.</param>
        /// <param name="type">Тип помилки (за замовчуванням "Error").</param>
        public CompilerException(string message, int line, int column, string type = "Error")
            : base(message)
        {
            Line = line;
            Column = column;
            ErrorType = type;
        }

        /// <summary>
        /// Повертає відформатований рядок помилки для виводу в консоль.
        /// Формат: [Type] Line X, Col Y: Message
        /// </summary>
        public override string ToString()
        {
            return $"[{ErrorType}] Line {Line}, Col {Column}: {Message}";
        }
    }
}