using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace darlet.Core.LexicalAnalysis
{
    /// <summary>
    /// Клас-контейнер для збереження результатів роботи лексера.
    /// Він не просто зберігає дані, а й керує процесом наповнення таблиць ідентифікаторів та констант під час аналізу.
    /// </summary>
    public class LexicalAnalysisResult
    {
        /// <summary>
        /// Чи пройшов аналіз без лексичних помилок.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Повний список усіх знайдених токенів (включно з операторами та ключовими словами).
        /// </summary>
        public List<Token> SymbolTable { get; } = new List<Token>();

        /// <summary>
        /// Таблиця унікальних ідентифікаторів (змінних). Ключ - ім'я, Значення - унікальний ID.
        /// </summary>
        public Dictionary<string, int> IdentifierTable { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Таблиця унікальних констант (числа, рядки, булеві значення).
        /// </summary>
        public Dictionary<string, int> ConstantTable { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Список помилок, знайдених під час сканування.
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        // Лічильники для генерації унікальних ID
        private int _nextIdentifierId = 1;
        private int _nextConstantId = 1;

        /// <summary>
        /// Запускає лексичний аналіз та структурує результати.
        /// </summary>
        /// <param name="lexer">Ініціалізований екземпляр лексера.</param>
        public LexicalAnalysisResult(Lexer lexer)
        {
            Token token;

            // Цикл перебору всіх токенів до кінця файлу
            do
            {
                token = lexer.GetNextToken();
                SymbolTable.Add(token);

                // 1. Реєстрація помилок
                if (token.Type == TokenType.UNKNOWN)
                {
                    Errors.Add($"Error at ({token.Line}:{token.Column}): Unknown token '{token.Lexeme}'");
                }

                // 2. Наповнення таблиці ідентифікаторів
                // Якщо токен - це змінна, додаємо її в окрему таблицю, якщо її там ще немає
                if (token.Type == TokenType.IDENTIFIER)
                {
                    if (!IdentifierTable.ContainsKey(token.Lexeme))
                    {
                        IdentifierTable.Add(token.Lexeme, _nextIdentifierId++);
                    }
                }

                // 3. Наповнення таблиці констант
                // Якщо токен - це літерал, додаємо його в таблицю констант
                if (IsConstant(token.Type))
                {
                    if (!ConstantTable.ContainsKey(token.Lexeme))
                    {
                        ConstantTable.Add(token.Lexeme, _nextConstantId++);
                    }
                }

            } while (token.Type != TokenType.EOF);

            // Успіх, якщо список помилок порожній
            Success = Errors.Count == 0;
        }

        /// <summary>
        /// Перевіряє, чи є тип токена константою (число, рядок, булеве значення).
        /// </summary>
        private bool IsConstant(TokenType type)
        {
            return type == TokenType.INTEGER_LITERAL ||
                   type == TokenType.REAL_LITERAL ||
                   type == TokenType.BOOLEAN_LITERAL ||
                   type == TokenType.STRING_LITERAL;
        }

        /// <summary>
        /// Виводить детальний звіт про результати аналізу в консоль.
        /// Корисно для налагодження та демонстрації роботи лексера.
        /// </summary>
        public void PrintResults()
        {
            // 1. Вивід загальної послідовності токенів
            Console.WriteLine("--- Table of Symbols ---");
            Console.WriteLine("{0,-5} {1,-10} {2,-25} {3,-20} {4,-5}", "#", "Line", "Lexeme", "Token Type", "Index");
            Console.WriteLine(new string('-', 70));

            int symbolIndex = 1;
            foreach (var t in SymbolTable)
            {
                if (t.Type == TokenType.EOF) continue;

                string index = "";

                // Якщо токен має посилання на додаткову таблицю, показуємо його ID
                if (t.Type == TokenType.IDENTIFIER)
                {
                    index = IdentifierTable[t.Lexeme].ToString();
                }
                else if (IsConstant(t.Type))
                {
                    index = ConstantTable[t.Lexeme].ToString();
                }

                Console.WriteLine("{0,-5} {1,-10} {2,-25} {3,-20} {4,-5}",
                    symbolIndex++, t.Line, $"'{t.Lexeme}'", t.Type, index);
            }
            Console.WriteLine();

            // 2. Вивід унікальних ідентифікаторів
            Console.WriteLine("--- Identifier Table ---");
            Console.WriteLine("{0,-5} {1,-25}", "ID", "Name");
            Console.WriteLine(new string('-', 35));
            foreach (var entry in IdentifierTable)
            {
                Console.WriteLine("{0,-5} {1,-25}", entry.Value, entry.Key);
            }
            Console.WriteLine();

            // 3. Вивід унікальних констант
            Console.WriteLine("--- Constant Table ---");
            Console.WriteLine("{0,-5} {1,-25}", "ID", "Value");
            Console.WriteLine(new string('-', 35));
            foreach (var entry in ConstantTable)
            {
                Console.WriteLine("{0,-5} {1,-25}", entry.Value, entry.Key);
            }
            Console.WriteLine();

            // 4. Підсумок
            if (Success)
            {
                Console.WriteLine("Lexical analysis completed successfully.");
            }
            else
            {
                Console.WriteLine("Lexical analysis failed with the following errors:");
                foreach (var error in Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}