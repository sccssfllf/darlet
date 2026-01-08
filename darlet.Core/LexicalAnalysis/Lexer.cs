using System;
using System.Collections.Generic;
using System.Text;

namespace darlet.Core.LexicalAnalysis
{
    /// <summary>
    /// Лексичний аналізатор (Scanner).
    /// Відповідає за перший етап компіляції: розбиття вхідного тексту програми на послідовність значущих елементів (токенів).
    /// </summary>
    public class Lexer
    {
        private readonly string _source;        // Вхідний код програми
        private int _position;                  // Поточний індекс символу в рядку _source
        private int _line;                      // Поточний номер рядка (для повідомлень про помилки)
        private int _column;                    // Поточний номер стовпця

        // Словник для швидкого пошуку зарезервованих слів (O(1))
        private readonly Dictionary<string, TokenType> _keywords;

        /// <summary>
        /// Ініціалізує новий екземпляр лексера.
        /// </summary>
        /// <param name="source">Вихідний код програми.</param>
        public Lexer(string source)
        {
            _source = source;
            _position = 0;
            _line = 1;
            _column = 1;

            // Заповнюємо словник ключових слів, які підтримує мова Dartlet
            _keywords = new Dictionary<string, TokenType>
            {
                { "var", TokenType.KW_VAR },
                { "if", TokenType.KW_IF },
                { "else", TokenType.KW_ELSE },
                { "while", TokenType.KW_WHILE },
                { "print", TokenType.KW_PRINT },
                { "input", TokenType.KW_INPUT },
                { "return", TokenType.KW_RETURN },
                { "true", TokenType.BOOLEAN_LITERAL }, // Булеві значення трактуємо як літерали
                { "false", TokenType.BOOLEAN_LITERAL }
            };
        }

        /// <summary>
        /// Запускає процес лексичного аналізу для всього тексту.
        /// </summary>
        /// <returns>Список усіх знайдених токенів.</returns>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token token;

            // Читаємо токени по одному, доки не дійдемо до кінця файлу (EOF)
            do
            {
                token = GetNextToken();

                // EOF (End Of File) зазвичай не додається у список, він слугує маркером зупинки
                if (token.Type != TokenType.EOF)
                {
                    tokens.Add(token);
                }
            } while (token.Type != TokenType.EOF);

            return tokens;
        }

        // ----------------------------------------------------
        // Службові методи навігації (Source Reader)
        // ----------------------------------------------------

        /// <summary>
        /// "Підглядає" поточний символ, не пересуваючи курсор читання.
        /// </summary>
        private char Peek()
        {
            if (_position >= _source.Length)
                return '\0'; // Повертаємо нульовий символ як ознаку кінця файлу

            return _source[_position];
        }

        /// <summary>
        /// Повертає поточний символ і пересуває курсор на наступну позицію.
        /// Також оновлює лічильники рядків та колонок.
        /// </summary>
        private char Advance()
        {
            char current = Peek();
            if (current != '\0')
            {
                _position++;
                // Обробка переходу на новий рядок
                if (current == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
            }
            return current;
        }

        // ----------------------------------------------------
        // Основна логіка Лексера (Головний цикл автомата)
        // ----------------------------------------------------

        /// <summary>
        /// Знаходить та повертає наступний токен у потоці.
        /// Реалізує логіку кінцевого автомата.
        /// </summary>
        public Token GetNextToken()
        {
            while (true)
            {
                // 1. Пропуск "сміття" (пробіли, табуляція, ентери)
                if (IsWhitespace(Peek()))
                {
                    SkipWhitespace();
                    continue; // Повертаємось на початок циклу, щоб знайти значущий символ
                }

                // 2. Перевірка на кінець файлу
                if (Peek() == '\0')
                    return new Token(TokenType.EOF, "", _line, _column);

                // Запам'ятовуємо позицію початку токена для коректного повідомлення про помилки
                int startLine = _line;
                int startColumn = _column;

                char c = Peek();

                // 3. Вибір стратегії сканування залежно від символу

                // Якщо цифра -> це число (ціле або дійсне)
                if (IsDigit(c))
                    return ScanNumber(startLine, startColumn);

                // Якщо літера або '_' -> це ідентифікатор або ключове слово
                if (IsLetterOrUnderscore(c))
                    return ScanIdentifierOrKeyword(startLine, startColumn);

                // Якщо лапки -> це рядковий літерал
                if (c == '"')
                    return ScanString(startLine, startColumn);

                // Якщо слеш -> це може бути коментар (//) або ділення (/)
                if (c == '/')
                {
                    if (HandleSlashOperatorOrComment())
                        continue; // Це був коментар, шукаємо далі

                    // Це було ділення
                    Advance();
                    return new Token(TokenType.OP_DIVIDE, "/", startLine, startColumn);
                }

                // 4. Обробка символів-операторів
                switch (c)
                {
                    case '=':
                        Advance();
                        // Перевірка на "=="
                        if (Peek() == '=')
                        {
                            Advance();
                            return new Token(TokenType.OP_EQUAL, "==", startLine, startColumn);
                        }
                        return new Token(TokenType.OP_ASSIGN, "=", startLine, startColumn);

                    case '!':
                        Advance();
                        // Перевірка на "!="
                        if (Peek() == '=')
                        {
                            Advance();
                            return new Token(TokenType.OP_NOT_EQUAL, "!=", startLine, startColumn);
                        }
                        return new Token(TokenType.UNKNOWN, "!", startLine, startColumn); // Одинарний '!' поки не підтримується як NOT

                    case '<':
                    case '>':
                        return HandleComparisonOperator(c, startLine, startColumn);

                    // Односимвольні токени
                    case '+': Advance(); return new Token(TokenType.OP_PLUS, "+", startLine, startColumn);
                    case '-': Advance(); return new Token(TokenType.OP_MINUS, "-", startLine, startColumn);
                    case '*': Advance(); return new Token(TokenType.OP_MULTIPLY, "*", startLine, startColumn);
                    case '^': Advance(); return new Token(TokenType.OP_POWER, "^", startLine, startColumn);
                    case '(': Advance(); return new Token(TokenType.LPAREN, "(", startLine, startColumn);
                    case ')': Advance(); return new Token(TokenType.RPAREN, ")", startLine, startColumn);
                    case '{': Advance(); return new Token(TokenType.LBRACE, "{", startLine, startColumn);
                    case '}': Advance(); return new Token(TokenType.RBRACE, "}", startLine, startColumn);
                    case ';': Advance(); return new Token(TokenType.SEMICOLON, ";", startLine, startColumn);

                    default:
                        // Якщо символ не розпізнано
                        Advance();
                        return new Token(TokenType.UNKNOWN, c.ToString(), startLine, startColumn);
                }
            }
        }

        // ----------------------------------------------------
        // Допоміжні функції (Character Classification)
        // ----------------------------------------------------

        private bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';

        private void SkipWhitespace()
        {
            while (IsWhitespace(Peek()))
            {
                Advance();
            }
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        private bool IsLetterOrUnderscore(char c) => IsLetter(c) || c == '_';

        private bool IsAlphanumeric(char c) => IsLetterOrUnderscore(c) || IsDigit(c);

        // ----------------------------------------------------
        // Реалізація Спеціалізованих Станів (Scanners)
        // ----------------------------------------------------

        /// <summary>
        /// Визначає, чи поточний символ є початком коментаря, чи оператором ділення.
        /// </summary>
        /// <returns>True, якщо це коментар (був пропущений). False, якщо це оператор.</returns>
        private bool HandleSlashOperatorOrComment()
        {
            // Перевіряємо наявність другого слеша: //
            if (_position + 1 < _source.Length && _source[_position + 1] == '/')
            {
                Advance(); // ' / '
                Advance(); // ' / '

                // Пропускаємо все до кінця рядка
                while (Peek() != '\n' && Peek() != '\0')
                {
                    Advance();
                }

                // Пропускаємо символ нового рядка, щоб оновити лічильник _line
                if (Peek() == '\n')
                {
                    Advance();
                }

                return true; // Це був коментар
            }

            return false; // Це не коментар
        }

        /// <summary>
        /// Сканує числові літерали (цілі та з плаваючою крапкою).
        /// </summary>
        private Token ScanNumber(int startLine, int startColumn)
        {
            int startPos = _position;

            // 1. Читаємо цілу частину
            while (IsDigit(Peek()))
            {
                Advance();
            }

            // 2. Перевіряємо на наявність дробової частини (Real)
            if (Peek() == '.')
            {
                Advance(); // Проходимо крапку

                // Валідація: після крапки має бути хоча б одна цифра (наприклад, "123." - помилка)
                if (!IsDigit(Peek()))
                {
                    string lexeme = _source.Substring(startPos, _position - startPos);
                    return new Token(TokenType.UNKNOWN, lexeme, startLine, startColumn);
                }

                while (IsDigit(Peek()))
                {
                    Advance();
                }

                string realLexeme = _source.Substring(startPos, _position - startPos);
                return new Token(TokenType.REAL_LITERAL, realLexeme, startLine, startColumn);
            }

            // Це було ціле число
            string intLexeme = _source.Substring(startPos, _position - startPos);
            return new Token(TokenType.INTEGER_LITERAL, intLexeme, startLine, startColumn);
        }

        /// <summary>
        /// Сканує ідентифікатори (змінні) або ключові слова.
        /// </summary>
        private Token ScanIdentifierOrKeyword(int startLine, int startColumn)
        {
            int startPos = _position;

            // Читаємо слово повністю (літери + цифри)
            while (IsAlphanumeric(Peek()))
            {
                Advance();
            }

            string lexeme = _source.Substring(startPos, _position - startPos);

            // Якщо слово є у словнику - це Keyword, інакше - Identifier
            if (_keywords.TryGetValue(lexeme, out TokenType type))
            {
                return new Token(type, lexeme, startLine, startColumn);
            }

            return new Token(TokenType.IDENTIFIER, lexeme, startLine, startColumn);
        }

        /// <summary>
        /// Сканує рядкові літерали у подвійних лапках.
        /// </summary>
        private Token ScanString(int startLine, int startColumn)
        {
            Advance(); // Пропускаємо відкриваючу лапку
            int startPos = _position;

            // Читаємо до закриваючої лапки
            while (Peek() != '"' && Peek() != '\0')
            {
                Advance();
            }

            // Помилка: рядок не закрито
            if (Peek() == '\0')
            {
                string lexeme = _source.Substring(startPos - 1, _position - startPos + 1);
                return new Token(TokenType.UNKNOWN, "Unterminated string: " + lexeme, startLine, startColumn);
            }

            // Витягуємо вміст рядка (без лапок)
            string lexemeValue = _source.Substring(startPos, _position - startPos);
            Advance(); // Пропускаємо закриваючу лапку

            return new Token(TokenType.STRING_LITERAL, lexemeValue, startLine, startColumn);
        }

        /// <summary>
        /// Обробляє оператори порівняння (<, >, <=, >=).
        /// </summary>
        private Token HandleComparisonOperator(char c, int startLine, int startColumn)
        {
            TokenType singleType = c == '<' ? TokenType.OP_LESS : TokenType.OP_GREATER;
            TokenType equalType = c == '<' ? TokenType.OP_LESS_EQUAL : TokenType.OP_GREATER_EQUAL;
            string singleLexeme = c.ToString();
            string equalLexeme = singleLexeme + "=";

            Advance(); // Споживаємо перший символ

            // Перевіряємо наявність '='
            if (Peek() == '=')
            {
                Advance();
                return new Token(equalType, equalLexeme, startLine, startColumn);
            }

            return new Token(singleType, singleLexeme, startLine, startColumn);
        }
    }
}