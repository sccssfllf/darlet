using System;
using System.Collections.Generic;
using System.Text;

namespace darlet.Core.LexicalAnalysis;

public class Lexer
{
    private readonly string _source;        // Вхідний код Dartlet
    private int _position;                  // Поточна позиція читання (індекс символу)
    private int _line;                      // Поточний рядок
    private int _column;                    // Поточний стовпець

    // Таблиця ключових слів для швидкого розпізнавання
    private readonly Dictionary<string, TokenType> _keywords;

    public Lexer(string source)
    {
        _source = source;
        _position = 0;
        _line = 1;
        _column = 1;

        // Ініціалізація таблиці ключових слів
        _keywords = new Dictionary<string, TokenType>
        {
            { "var", TokenType.KW_VAR },
            { "if", TokenType.KW_IF },
            { "else", TokenType.KW_ELSE },
            { "while", TokenType.KW_WHILE },
            { "print", TokenType.KW_PRINT },
            { "input", TokenType.KW_INPUT },
            { "return", TokenType.KW_RETURN },
            { "true", TokenType.BOOLEAN_LITERAL },
            { "false", TokenType.BOOLEAN_LITERAL }
        };
    }
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        Token token;

        do
        {
            token = GetNextToken();
            // Ми не додаємо EOF у список, парсер сам зрозуміє, коли список закінчиться
            if (token.Type != TokenType.EOF)
            {
                tokens.Add(token);
            }
        } while (token.Type != TokenType.EOF);

        return tokens;
    }


    // ----------------------------------------------------
    // Службові методи (Source Reader)
    // ----------------------------------------------------

    // Повертає поточний символ без переміщення курсору
    private char Peek()
    {
        if (_position >= _source.Length)
            return '\0'; // Повертаємо '\0' як EOF

        return _source[_position];
    }

    // Повертає поточний символ і переміщує курсор на одну позицію вперед
    private char Advance()
    {
        char current = Peek();
        if (current != '\0')
        {
            _position++;
            if (current == '\n')
            {
                _line++;
                _column = 1; // Скидаємо стовпець на початку нового рядка
            }
            else
            {
                _column++;
            }
        }
        return current;
    }

    // ----------------------------------------------------
    // Основна логіка Лексера (Діаграма Станів)
    // ----------------------------------------------------

    public Token GetNextToken()
    {
        // Головна петля автомата, яка пропускає пробіли та коментарі
        while (true)
        {
            // Пропускаємо пробіли та переходи рядка
            if (IsWhitespace(Peek()))
            {
                SkipWhitespace();
                continue;
            }

            // Перевірка на EOF
            if (Peek() == '\0')
                return new Token(TokenType.EOF, "", _line, _column);

            // Запам'ятовуємо позицію початку токена
            int startLine = _line;
            int startColumn = _column;

            char c = Peek();

            // ----------------------------------------------------
            // 1. Обробка чисел, ідентифікаторів, рядків та операторів (Автомат)
            // ----------------------------------------------------

            if (IsDigit(c))
                return ScanNumber(startLine, startColumn);

            if (IsLetterOrUnderscore(c))
                return ScanIdentifierOrKeyword(startLine, startColumn);

            if (c == '"')
                return ScanString(startLine, startColumn);

            // Перевірка на коментарі та оператори
            if (c == '/')
            {
                if (HandleSlashOperatorOrComment())
                    continue; // Продовжуємо петлю, якщо обробили коментар

                // Якщо це був не коментар, повертаємо оператор ділення
                Advance();
                return new Token(TokenType.OP_DIVIDE, "/", startLine, startColumn);
            }

            // ----------------------------------------------------
            // 2. Обробка простих операторів та роздільників
            // ----------------------------------------------------

            switch (c)
            {
                // Обробка складних операторів (==, !=, <=, >=) та присвоєння (=)
                case '=':
                    Advance(); // Споживаємо '='
                    if (Peek() == '=')
                    {
                        Advance();
                        return new Token(TokenType.OP_EQUAL, "==", startLine, startColumn);
                    }
                    return new Token(TokenType.OP_ASSIGN, "=", startLine, startColumn);

                case '!':
                    Advance();
                    if (Peek() == '=')
                    {
                        Advance();
                        return new Token(TokenType.OP_NOT_EQUAL, "!=", startLine, startColumn);
                    }
                    // Якщо після '!' немає '=', це може бути помилка або непідтримуваний оператор
                    return new Token(TokenType.UNKNOWN, "!", startLine, startColumn);

                case '<':
                case '>':
                    return HandleComparisonOperator(c, startLine, startColumn);

                // Обробка простих операторів та роздільників
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
                    // Невідомий символ - повертаємо UNKNOWN
                    Advance(); // Споживаємо невідомий символ
                    return new Token(TokenType.UNKNOWN, c.ToString(), startLine, startColumn);
            }
        }
    }

    // ----------------------------------------------------
    // 3. Допоміжні функції для станів (Діаграма станів)
    // ----------------------------------------------------

    private bool IsWhitespace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r';
    }

    private void SkipWhitespace()
    {
        while (IsWhitespace(Peek()))
        {
            Advance();
        }
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsLetter(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    private bool IsLetterOrUnderscore(char c)
    {
        return IsLetter(c) || c == '_';
    }

    private bool IsAlphanumeric(char c)
    {
        return IsLetterOrUnderscore(c) || IsDigit(c);
    }

    // ----------------------------------------------------
    // 4. Реалізація Спеціалізованих Станів (Діаграма станів)
    // ----------------------------------------------------

    // Стан для обробки коментарів (//)
    private bool HandleSlashOperatorOrComment()
    {
        // Перевіряємо, чи це коментар: //
        if (_position + 1 < _source.Length && _source[_position + 1] == '/')
        {
            Advance(); // Споживаємо першу '/'
            Advance(); // Споживаємо другу '/' (Вхід у стан коментаря)

            // Цикл ігнорує всі символи до кінця рядка або EOF
            while (Peek() != '\n' && Peek() != '\0')
            {
                Advance();
            }
            // Споживаємо '\n' (якщо не EOF), щоб правильно встановити _line і _column
            if (Peek() == '\n')
            {
                Advance();
            }

            // Повертаємо true, щоб головна петля викликала GetNextToken() знову
            return true;
        }

        // Це був не коментар, а оператор ділення: /
        return false;
    }

    private Token ScanNumber(int startLine, int startColumn)
    {
        int startPos = _position;

        // 1. Читаємо цілу частину
        while (IsDigit(Peek()))
        {
            Advance();
        }

        // 2. Перевіряємо на дробову частину ('.' - перехід до REAL_LITERAL)
        if (Peek() == '.')
        {
            Advance(); // Споживаємо '.'

            // Перевіряємо, чи є хоча б одна цифра після крапки
            if (!IsDigit(Peek()))
            {
                // Помилка: 123. не є коректним числом
                string lexeme = _source.Substring(startPos, _position - startPos);
                return new Token(TokenType.UNKNOWN, lexeme, startLine, startColumn);
            }

            while (IsDigit(Peek()))
            {
                Advance();
            }
            // Виділяємо токен REAL_LITERAL
            string realLexeme = _source.Substring(startPos, _position - startPos);
            return new Token(TokenType.REAL_LITERAL, realLexeme, startLine, startColumn);
        }

        // Виділяємо токен INTEGER_LITERAL
        string intLexeme = _source.Substring(startPos, _position - startPos);
        return new Token(TokenType.INTEGER_LITERAL, intLexeme, startLine, startColumn);
    }

    private Token ScanIdentifierOrKeyword(int startLine, int startColumn)
    {
        int startPos = _position;

        // Читаємо всі наступні літери, цифри та підкреслення
        while (IsAlphanumeric(Peek()))
        {
            Advance();
        }

        string lexeme = _source.Substring(startPos, _position - startPos);

        // Перевіряємо, чи є лексема ключовим словом
        if (_keywords.TryGetValue(lexeme, out TokenType type))
        {
            return new Token(type, lexeme, startLine, startColumn);
        }

        // Якщо ні, це звичайний ідентифікатор
        return new Token(TokenType.IDENTIFIER, lexeme, startLine, startColumn);
    }

    private Token ScanString(int startLine, int startColumn)
    {
        // 1. Споживаємо відкриваючу лапку (вхід у стан S_STRING)
        Advance();
        int startPos = _position;

        // 2. Читаємо до кінцевої лапки або EOF
        while (Peek() != '"' && Peek() != '\0')
        {
            // У складнішому випадку тут має бути обробка екранування (\")
            Advance();
        }

        // Якщо дійшли до EOF без закриваючої лапки - це помилка
        if (Peek() == '\0')
        {
            string lexeme = _source.Substring(startPos - 1, _position - startPos + 1);
            return new Token(TokenType.UNKNOWN, "Unterminated string: " + lexeme, startLine, startColumn);
        }

        // 3. Виділяємо рядок (без лапок)
        string lexemeValue = _source.Substring(startPos, _position - startPos);
        Advance(); // Споживаємо кінцеву лапку

        return new Token(TokenType.STRING_LITERAL, lexemeValue, startLine, startColumn);
    }

    private Token HandleComparisonOperator(char c, int startLine, int startColumn)
    {
        TokenType singleType = c == '<' ? TokenType.OP_LESS : TokenType.OP_GREATER;
        TokenType equalType = c == '<' ? TokenType.OP_LESS_EQUAL : TokenType.OP_GREATER_EQUAL;
        string singleLexeme = c.ToString();
        string equalLexeme = singleLexeme + "=";

        Advance(); // Споживаємо '<' або '>'

        if (Peek() == '=')
        {
            Advance();
            return new Token(equalType, equalLexeme, startLine, startColumn);
        }
        // Це був одиночний оператор (< або >)
        return new Token(singleType, singleLexeme, startLine, startColumn);
    }
}