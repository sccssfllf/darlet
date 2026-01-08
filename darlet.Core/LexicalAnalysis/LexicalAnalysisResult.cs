using System.Runtime.CompilerServices;

namespace darlet.Core.LexicalAnalysis;

// Клас для зберігання результатів лексичного аналізу
public class LexicalAnalysisResult
{
    public bool Success { get; }
    public List<Token> SymbolTable { get; } = new List<Token>();
    public Dictionary<string, int> IdentifierTable { get; } = new Dictionary<string, int>();
    public Dictionary<string, int> ConstantTable { get; } = new Dictionary<string, int>();
    public List<string> Errors { get; } = new List<string>();

    private int _nextIdentifierId = 1;
    private int _nextConstantId = 1;

    public LexicalAnalysisResult(Lexer lexer)
    {
        Token token;

        do
        {
            token = lexer.GetNextToken();
            SymbolTable.Add(token);

            // Обробка помилок
            if (token.Type == TokenType.UNKNOWN)
            {
                Errors.Add($"Error at ({token.Line}:{token.Column}): Unknown token '{token.Lexeme}'");
            }

            // Заповнення таблиці ідентифікаторів
            if (token.Type == TokenType.IDENTIFIER)
            {
                if (!IdentifierTable.ContainsKey(token.Lexeme))
                {
                    IdentifierTable.Add(token.Lexeme, _nextIdentifierId++);
                }
            }

            // Заповнення таблиці констант
            if (IsConstant(token.Type))
            {
                if (!ConstantTable.ContainsKey(token.Lexeme))
                {
                    ConstantTable.Add(token.Lexeme, _nextConstantId++);
                }
            }

        } while (token.Type != TokenType.EOF);

        Success = Errors.Count == 0;
    }

    private bool IsConstant(TokenType type)
    {
        return type == TokenType.INTEGER_LITERAL ||
               type == TokenType.REAL_LITERAL ||
               type == TokenType.BOOLEAN_LITERAL ||
               type == TokenType.STRING_LITERAL;
    }

    public void PrintResults()
    {
        // 1. Вивід загальної таблиці розбору
        Console.WriteLine("--- Table of Symbols ---");
        Console.WriteLine("{0,-5} {1,-10} {2,-25} {3,-20} {4,-5}", "#", "Line", "Lexeme", "Token Type", "Index");
        Console.WriteLine(new string('-', 70));
        int symbolIndex = 1;
        foreach (var t in SymbolTable)
        {
            if (t.Type == TokenType.EOF) continue; // Не виводимо EOF у таблицю

            string index = "";
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

        // 2. Вивід таблиці ідентифікаторів
        Console.WriteLine("--- Identifier Table ---");
        Console.WriteLine("{0,-5} {1,-25}", "ID", "Name");
        Console.WriteLine(new string('-', 35));
        foreach (var entry in IdentifierTable)
        {
            Console.WriteLine("{0,-5} {1,-25}", entry.Value, entry.Key);
        }
        Console.WriteLine();

        // 3. Вивід таблиці констант
        Console.WriteLine("--- Constant Table ---");
        Console.WriteLine("{0,-5} {1,-25}", "ID", "Value");
        Console.WriteLine(new string('-', 35));
        foreach (var entry in ConstantTable)
        {
            Console.WriteLine("{0,-5} {1,-25}", entry.Value, entry.Key);
        }
        Console.WriteLine();

        // 4. Вивід повідомлення про успіх або помилки
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
